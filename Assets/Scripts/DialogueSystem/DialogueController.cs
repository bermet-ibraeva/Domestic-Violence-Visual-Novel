using System.Collections.Generic;
using UnityEngine;

/*
Dialogue Controller

This class manages the dialogue system for the visual novel.

Main responsibilities:
- Loads episode data from Resources using EpisodeLoader
- Builds dictionaries for quick access to nodes and scenes
- Controls dialogue flow between nodes
- Displays dialogue text using UIController
- Manages character portraits (left and right slots)
- Applies scene rules:
    • left character is fixed per scene
    • only allowed right characters can appear
- Handles narrator lines (hides both character portraits)
- Changes backgrounds and background effects
- Processes player choices and branching paths
- Applies node effects only once per save file
- Applies choice effects when a player selects an option
- Supports jumping between nodes across different scenes
- Automatically saves progress after each node
- Restores progress using SaveSystem
*/

public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    public UIController ui;

    [Header("Portrait Slots")]
    public GameObject LeftCharacter;
    public CharacterPortraitController LeftPortrait;

    public GameObject RightCharacter;
    public CharacterPortraitController RightPortrait;

    [Header("Portrait Rules")]
    public bool HideLeftWhenRightSpeaks = true;
    public bool HideRightWhenLeftSpeaks = true;

    [Header("Layout")]
    public LayoutController layout;

    // Эти цвета теперь можно применять внутри методов Show, если нужно 
    // (или настроить их один раз в префабах панелей)
    [Header("Text Colors")]
    public Color AuthorColor = Color.gray;
    public Color LeftCharacterColor = Color.white;
    public Color OtherColor = Color.white;

    [Header("Backgrounds")]
    public BackgroundController backgroundController;

    [Header("Episode Settings")]
    public string episodePath = "Episodes/episode_1";
    public string startNodeId = "E01_S01_start";

    [Header("Chapter")]
    public int chapterNumber = 1;

    [Header("Episode End Panel UI")]
    public EpisodeEndPanel episodeEndPanel;

    [Header("Stat Feedback UI")]
    public StatFeedbackUI statFeedbackUI;

    private EpisodeData episode;
    private Dictionary<string, DialogueNode> nodeDict;
    private Dictionary<string, SceneData> sceneDict;
    private Dictionary<string, SceneData> nodeToScene;

    private SceneData currentScene;
    private readonly HashSet<string> currentRightAllowed = new HashSet<string>();

    private DialogueNode currentNode;
    private bool waitingForAdvance = false;
    private SaveData save;

    void Start()
    {
        save = TempGameContext.saveToLoad ?? SaveSystem.Load();
        bool hasLoadedSave = save != null;

        if (save == null)
            save = new SaveData();

        if (save.appliedEffectNodes == null)
            save.appliedEffectNodes = new List<string>();

        string requestedEpisodePath = episodePath;
        string requestedStartNodeId = startNodeId;

        bool sameEpisodeAsSave =
            hasLoadedSave &&
            !string.IsNullOrEmpty(save.episodePath) &&
            save.episodePath == requestedEpisodePath;

        bool hasMidEpisodeProgress =
            hasLoadedSave &&
            sameEpisodeAsSave &&
            !string.IsNullOrEmpty(save.currentNodeId) &&
            save.currentNodeId != requestedStartNodeId;

        if (sameEpisodeAsSave)
        {
            episodePath = save.episodePath;
            startNodeId = string.IsNullOrEmpty(save.currentNodeId) ? requestedStartNodeId : save.currentNodeId;
        }
        else
        {
            episodePath = requestedEpisodePath;
            startNodeId = requestedStartNodeId;
        }

        LoadEpisode();

        StatSystem.Instance.Init(save);

        if (episodeEndPanel != null)
        {
            episodeEndPanel.Init(this);
            episodeEndPanel.Hide();
        }

        if (!hasMidEpisodeProgress)
        {
            StatSystem.Instance.ResetEpisodeStats();
            save.episodeRewardGranted = false;
            save.currentNodeId = startNodeId;
            save.episodePath = episodePath;

            save.episodeStartSnapshot = new EpisodeSnapshot
            {
                sparksTotal = save.sparksTotal,
                trustAG = save.trustAG,
                trustJA = save.trustJA,
                riskTotal = save.riskTotal,
                safetyTotal = save.safetyTotal
            };
        }

        ShowNode(startNodeId);
    }

    void Update()
    {
        if (waitingForAdvance && Input.GetMouseButtonDown(0))
        {
            waitingForAdvance = false;
            if (currentNode == null) return;

            if (!string.IsNullOrEmpty(currentNode.nextNode))
                ShowNode(currentNode.nextNode);
            else
                ui.HideChoices();
        }
    }

    void LoadEpisode()
    {
        episode = EpisodeLoader.LoadEpisode(episodePath, out nodeDict, out sceneDict, out nodeToScene);
        if (episode == null)
            Debug.LogError("[DialogueController] Episode failed to load");
    }

    // ---------------- Helpers ----------------

    bool IsNarrator(string ch) => string.IsNullOrEmpty(ch) || ch == "Narrator" || ch == "Автор";
    string NormalizeEmotion(string em) => string.IsNullOrEmpty(em) ? "Calm" : em;

    void HideLeft() { if (LeftCharacter != null) LeftCharacter.SetActive(false); }
    void ShowLeft(string characterName, string emotion)
    {
        if (LeftCharacter != null) LeftCharacter.SetActive(true);
        LeftPortrait?.Show(characterName, NormalizeEmotion(emotion));
    }

    void HideRight() { if (RightCharacter != null) RightCharacter.SetActive(false); }
    void ShowRight(string characterName, string emotion)
    {
        if (RightCharacter != null) RightCharacter.SetActive(true);
        RightPortrait?.Show(characterName, NormalizeEmotion(emotion));
    }

    void HideAllCharacters() { HideLeft(); HideRight(); }

    void RebuildRightAllowed(SceneData scene)
    {
        currentRightAllowed.Clear();
        if (scene == null || scene.rightCharacters == null) return;
        foreach (var c in scene.rightCharacters)
            if (!string.IsNullOrEmpty(c)) currentRightAllowed.Add(c);
    }

    void EnterSceneIfChanged(string nodeId)
    {
        if (nodeToScene == null || !nodeToScene.TryGetValue(nodeId, out var newScene) || newScene == null)
            return;

        bool sceneChanged = (currentScene == null) || (currentScene.sceneId != newScene.sceneId);
        if (!sceneChanged) return;

        currentScene = newScene;
        RebuildRightAllowed(currentScene);

        if (backgroundController != null && !string.IsNullOrEmpty(currentScene.background))
            backgroundController.ApplyBackground(currentScene.background, currentScene.bgFx);

        if (!string.IsNullOrEmpty(currentScene.leftCharacter))
            ShowLeft(currentScene.leftCharacter, "Calm");
        else
            HideLeft();

        HideRight();
    }

    bool IsLeftCharacter(string ch) => currentScene != null && !string.IsNullOrEmpty(currentScene.leftCharacter) && ch == currentScene.leftCharacter;
    bool IsRightAllowed(string ch) => !string.IsNullOrEmpty(ch) && currentRightAllowed.Contains(ch);

    // ---------------- Main ----------------
    void ShowNode(string nodeId)
    {
        if (nodeDict == null || !nodeDict.TryGetValue(nodeId, out var node) || node == null)
        {
            Debug.LogError("[DialogueController] Node not found: " + nodeId);
            return;
        }

        EnterSceneIfChanged(nodeId);
        currentNode = node;
        waitingForAdvance = false;

        if (IsSummaryScreenNode(node))
        {
            ShowEpisodeEndPanel();
            return;
        }

        ApplyEffectsOnce(nodeId, node);

        // Background
        if (backgroundController != null)
        {
            if (node.stopPreviousBgEffect) backgroundController.StopEffect();
            if (!string.IsNullOrEmpty(node.background)) backgroundController.ApplyBackground(node.background, node.bgFx);
            if (!string.IsNullOrEmpty(node.bgFx) && node.bgFx != "none") backgroundController.PlayEffect(node.bgFx);
        }

        // -------- Text + portraits (ИСПРАВЛЕНО ЗДЕСЬ) --------
        if (IsNarrator(node.character))
        {
            // Используем метод ShowAuthor вместо прямого доступа к Text
            ui.ShowAuthor(node.text);
            HideAllCharacters();
        }
        else if (IsLeftCharacter(node.character))
        {
            // Передаем имя из сцены и текст из ноды
            ui.ShowLeftCharacter(currentScene.leftCharacter, node.text);
            ShowLeft(currentScene.leftCharacter, node.emotion);

            if (HideRightWhenLeftSpeaks) HideRight();
        }
        else if (IsRightAllowed(node.character))
        {
            ui.ShowRightCharacter(node.character, node.text);
            ShowRight(node.character, node.emotion);

            if (HideLeftWhenRightSpeaks) HideLeft();
            else if (!string.IsNullOrEmpty(currentScene?.leftCharacter))
                ShowLeft(currentScene.leftCharacter, "Calm");
        }
        else
        {
            // Ошибка или сторонний персонаж (показываем справа)
            ui.ShowRightCharacter(node.character, node.text);
            HideAllCharacters();
        }

        // Choices
        SetupChoices(node);

        // Autosave
        save.episodePath = episodePath;
        save.currentNodeId = nodeId;
        save.chapterNumber = chapterNumber;
        SaveSystem.Save(save);
    }

    void SetupChoices(DialogueNode node)
    {
        bool hasChoices = node.choices != null && node.choices.Count > 0;
        if (hasChoices)
        {
            ui.ShowChoices(node.choices, OnChoicePicked);
            waitingForAdvance = false;
        }
        else
        {
            ui.HideChoices();
            waitingForAdvance = true;
        }
    }

    void OnChoicePicked(string nextNodeId)
    {
        waitingForAdvance = false;
        TryApplyPickedChoiceEffects(nextNodeId);
        if (!string.IsNullOrEmpty(nextNodeId)) ShowNode(nextNodeId);
        else ui.HideChoices();
    }

    // ... Остальные методы (ApplyEffects, HandleEnding и т.д.) без изменений
    void TryApplyPickedChoiceEffects(string nextNodeId)
    {
        if (currentNode == null || currentNode.choices == null || string.IsNullOrEmpty(nextNodeId)) return;
        Choice picked = currentNode.choices.Find(c => c != null && c.nextNode == nextNodeId);
        if (picked == null || picked.effects == null || picked.effects.Count == 0) return;
        ApplyChoiceEffects(picked.effects);
        SaveSystem.Save(save);
    }

    void ApplyEffectsOnce(string nodeId, DialogueNode node)
    {
        if (node == null || node.effects == null) return;
        string key = $"{episodePath}:{nodeId}";
        if (save.appliedEffectNodes.Contains(key)) return;
        ApplyEffects(node.effects);
        save.appliedEffectNodes.Add(key);
    }

    void ApplyEffects(NodeEffects e)
    {
        if (e == null) return;
        StatSystem.Instance.ApplyLegacyNodeEffects(e);
    }

    void ApplyChoiceEffects(List<EffectOp> ops)
    {
        if (ops == null || ops.Count == 0) return;

        StatSystem.Instance.ApplyChoiceEffects(ops);

        foreach (var op in ops)
        {
            if (op == null || op.value == 0)
                continue;

            switch (op.key)
            {
                case "trust.AG":
                case "trust.JA":
                case "risk":
                case "safety":
                    statFeedbackUI?.ShowStatChange(op.key, op.value);
                    break;
            }
        }
    }


    private string GetEpisodeStartNode()
    {
        if (episode != null && episode.scenes != null && episode.scenes.Count > 0)
        {
            if (!string.IsNullOrEmpty(episode.scenes[0].startNode))
                return episode.scenes[0].startNode;
        }

        return startNodeId;
    }

    bool IsSummaryScreenNode(DialogueNode node)
    {
        return node != null
            && node.character == "System"
            && node.text == "summary_screen";
    }

    void ShowEpisodeEndPanel()
    {
        Debug.Log("[DialogueController] ShowEpisodeEndPanel called");

        int reward = 2;

        if (!save.episodeRewardGranted)
        {
            StatSystem.Instance.AddEpisodeReward(reward);
            save.episodeRewardGranted = true;
            SaveSystem.Save(save);
        }

        ui.HideChoices();
        ui.HideDialoguePanels();
        HideAllCharacters();

        if (episodeEndPanel == null)
        {
            Debug.LogError("[DialogueController] episodeEndPanel is NULL");
            return;
        }

        string nextEpisodePath = GetNextEpisodePath();
        string nextEpisodeStartNode = GetNextEpisodeStartNode();

        Debug.Log("[DialogueController] Showing episode end panel");

        episodeEndPanel.Show(
            save,
            nextEpisodePath,
            nextEpisodeStartNode
        );
    }

    string GetNextEpisodeStartNode()
    {
        if (string.IsNullOrEmpty(episodePath))
            return null;

        string fileName = episodePath.Substring(episodePath.LastIndexOf('_') + 1);

        if (!int.TryParse(fileName, out int currentEpisodeNumber))
        {
            Debug.LogWarning("[DialogueController] Could not parse current episode number from path: " + episodePath);
            return null;
        }

        int nextEpisodeNumber = currentEpisodeNumber + 1;
        return $"E{nextEpisodeNumber:D2}_S01_start";
    }

    string GetNextEpisodePath()
    {
        if (string.IsNullOrEmpty(episodePath))
            return null;

        string fileName = episodePath.Substring(episodePath.LastIndexOf('_') + 1);

        if (!int.TryParse(fileName, out int currentEpisodeNumber))
        {
            Debug.LogWarning("[DialogueController] Could not parse current episode number from path: " + episodePath);
            return null;
        }

        int nextEpisodeNumber = currentEpisodeNumber + 1;
        return $"Episodes/episode_{nextEpisodeNumber}";
    }

    public void StartNextEpisode(string newEpisodePath, string newStartNodeId)
    {
        if (string.IsNullOrEmpty(newEpisodePath) || string.IsNullOrEmpty(newStartNodeId))
        {
            Debug.LogWarning("[DialogueController] Next episode path or start node is empty.");
            return;
        }

        episodePath = newEpisodePath;
        startNodeId = newStartNodeId;

        save.episodePath = episodePath;
        save.currentNodeId = startNodeId;
        save.episodeRewardGranted = false;

        if (save.appliedEffectNodes == null)
            save.appliedEffectNodes = new List<string>();
        else
            save.appliedEffectNodes.Clear();

        StatSystem.Instance.ResetEpisodeStats();

        save.episodeStartSnapshot = new EpisodeSnapshot {
            sparksTotal = save.sparksTotal,
            trustAG = save.trustAG,
            trustJA = save.trustJA,
            riskTotal = save.riskTotal,
            safetyTotal = save.safetyTotal
        };

        if (episodeEndPanel != null)
            episodeEndPanel.Hide();

        SaveSystem.Save(save);

        LoadEpisode();
        ShowNode(startNodeId);
    }

    // will be used later in Settings Page to allow players to restart the current episode
    public void RestartCurrentEpisode()
    {
        string firstNode = GetEpisodeStartNode();

        if (string.IsNullOrEmpty(firstNode))
        {
            Debug.LogError("[DialogueController] Cannot restart episode: start node not found.");
            return;
        }

        if (save.episodeStartSnapshot != null)
        {
            save.sparksTotal = save.episodeStartSnapshot.sparksTotal;
            save.trustAG = save.episodeStartSnapshot.trustAG;
            save.trustJA = save.episodeStartSnapshot.trustJA;
            save.riskTotal = save.episodeStartSnapshot.riskTotal;
            save.safetyTotal = save.episodeStartSnapshot.safetyTotal;
        }

        StatSystem.Instance.ResetEpisodeStats();

        save.episodeRewardGranted = false;
        save.currentNodeId = firstNode;

        if (save.appliedEffectNodes == null)
            save.appliedEffectNodes = new List<string>();
        else
            save.appliedEffectNodes.Clear();

        if (episodeEndPanel != null)
            episodeEndPanel.Hide();

        SaveSystem.Save(save);
        ShowNode(firstNode);
    }
}