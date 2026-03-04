using System.Collections.Generic;
using UnityEngine;

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
        if (save == null) save = new SaveData();
        if (save.appliedEffectNodes == null) save.appliedEffectNodes = new List<string>();

        if (!string.IsNullOrEmpty(save.episodePath))
            episodePath = save.episodePath;

        startNodeId = string.IsNullOrEmpty(save.currentNodeId) ? startNodeId : save.currentNodeId;

        LoadEpisode();
        ShowNode(startNodeId);
    }

    void Update()
    {
        if (waitingForAdvance && Input.GetMouseButtonDown(0))
        {
            waitingForAdvance = false;
            if (currentNode == null) return;

            if (currentNode.requirements != null &&
                currentNode.requirements.Length > 0 &&
                string.IsNullOrEmpty(currentNode.nextNode))
            {
                HandleEnding(currentNode);
                return;
            }

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
        save.trustAG += e.trustAG;
        save.trustJA += e.trustJA;
        save.riskTotal += e.risk;
        save.safetyTotal += e.safety;
        save.episodeRisk += e.risk;
        save.episodeSafety += e.safety;
        save.sparksTotal += e.sparks;
    }

    void ApplyChoiceEffects(List<EffectOp> ops)
    {
        if (ops == null) return;
        foreach (var op in ops)
        {
            if (op == null || string.IsNullOrEmpty(op.op) || string.IsNullOrEmpty(op.key) || op.op != "inc") continue;
            int v = op.value;
            switch (op.key)
            {
                case "risk": save.riskTotal += v; save.episodeRisk += v; break;
                case "safety": save.safetyTotal += v; save.episodeSafety += v; break;
                case "trust.Ainaz_Guldana": save.trustAG += v; break;
            }
        }
    }

    void HandleEnding(DialogueNode node)
    {
        if (node.requirements == null || node.requirements.Length == 0)
        {
            ui.HideChoices();
            return;
        }
        Debug.Log("ENDING: " + node.requirements[0].ending);
        ui.HideChoices();
    }
}