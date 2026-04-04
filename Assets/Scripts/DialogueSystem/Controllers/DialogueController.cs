using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
Dialogue Controller

This class manages the dialogue system for the visual novel.

Main responsibilities:
- Loads episode data from Resources using EpisodeLoader
- Builds dictionaries for quick access to nodes and scenes
- Controls dialogue flow between nodes
- Displays dialogue text using UIController
- Manages character portraits (left and right slots)

Scene rules:
- Left character is fixed per scene
- Only allowed right characters can appear
- Narrator lines hide both character portraits

Visual handling:
- Changes backgrounds and applies background effects (bgFx)
- Updates UI panels for author, left, and right characters

Choices & branching:
- Displays player choices
- Handles choice selection and transitions to the next node
- Applies choice effects (e.g., trust, risk, safety)

Effects system:
- Applies node effects only once per save file
- Applies choice effects immediately on selection

Notification system:
- Supports two types of notifications:
    • Modal — temporarily replaces dialogue (shown before or after node/choice)
    • Toast — short message shown over UI without interrupting flow
- Notifications can be triggered:
    • On nodes
    • On choices
- Modal notifications pause dialogue flow until closed
- Toast notifications do not interrupt dialogue flow
- Supports "showOnce" logic to prevent repeated notifications

Flow control:
- Allows jumping between nodes across different scenes
- Ensures correct scene transitions when node changes
- Waits for player input to advance dialogue

Saving system:
- Automatically saves progress after each node
- Restores progress using SaveSystem (nodeId, episodePath, stats)

Additional features:
- Supports episode end / summary screen nodes
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

    // Эти цвета теперь можно применять внутри методов Show, если нужно 
    // (или настроить их один раз в префабах панелей)
    public RectTransform topPanel;

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

    [Header("Notification Controller")]
    public NotificationController notificationController;

    private EpisodeData episode;
    private Dictionary<string, DialogueNode> nodeDict;
    private Dictionary<string, SceneData> sceneDict;
    private Dictionary<string, SceneData> nodeToScene;
    private Dictionary<string, CharacterMeta> characterMetaDict;

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

        if (save.shownNotificationIds == null)
            save.shownNotificationIds = new List<string>();

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
                sparks = save.sparksTotal,
                trustAG = save.trustAGTotal,
                trustJA = save.trustJATotal,
                risk = save.riskTotal,
                safety = save.safetyTotal
            };
        }

        ShowNode(startNodeId);
    }

    void Update()
    {
        if (notificationController != null && notificationController.IsModalShowing)
            return;

        if (!waitingForAdvance)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (IsPointerOverTopPanel())
            return;

        waitingForAdvance = false;

        if (currentNode == null)
            return;

        if (!string.IsNullOrEmpty(currentNode.nextNode))
            ShowNode(currentNode.nextNode);
        else
            ui.HideChoices();
    }

    bool IsPointerOverTopPanel()
    {
        if (topPanel == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            topPanel,
            Input.mousePosition,
            null
        );
    }

    void LoadEpisode()
    {
        episode = EpisodeLoader.LoadEpisode(episodePath, out nodeDict, out sceneDict, out nodeToScene);

        if (episode == null)
        {
            Debug.LogError("[DialogueController] Episode failed to load");
            return;
        }

        BuildCharacterMetaDict();
    }

    void BuildCharacterMetaDict()
    {
        characterMetaDict = new Dictionary<string, CharacterMeta>();

        if (episode.characters == null)
            return;

        foreach (var ch in episode.characters)
        {
            if (ch == null || string.IsNullOrEmpty(ch.characterId))
                continue;

            characterMetaDict[ch.characterId] = ch;
        }
    }

    // ---------------- Helpers ----------------

    bool IsNarrator(string ch) => string.IsNullOrEmpty(ch) || ch == "Narrator";
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
        if (scene == null || scene.rightCharacterIds == null) return;
        foreach (var c in scene.rightCharacterIds)
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

        if (!string.IsNullOrEmpty(currentScene.leftCharacterId))
        {
            var meta = GetCharacterMeta(currentScene.leftCharacterId);
            ShowLeft(meta?.portraitKey, "Calm");
        }
        else
        {
            HideLeft();
        }

        HideRight();
    }

    CharacterMeta GetCharacterMeta(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
            return null;

        if (characterMetaDict != null && characterMetaDict.TryGetValue(characterId, out var meta))
            return meta;

        Debug.LogWarning($"[DialogueController] CharacterMeta not found for '{characterId}'");
        return null;
    }

    bool IsLeftCharacter(string ch) => currentScene != null && !string.IsNullOrEmpty(currentScene.leftCharacterId) && ch == currentScene.leftCharacterId;
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

        if (HasAction(node))
        {
            HandleActionNode(node);
            return;
        }

        ApplyEffectsOnce(nodeId, node);

        if (node.notification != null && notificationController != null && ShouldShowNotification(node.notification))
        {
            string mode = string.IsNullOrWhiteSpace(node.notification.mode)
                ? ""
                : node.notification.mode.Trim().ToLower();

            if (mode == "modal")
            {
                Debug.Log($"NODE notification found. id={node.notification.id}, mode={node.notification.mode}, shouldShow={ShouldShowNotification(node.notification)}");
                ui.HideChoices();
                ui.HideDialoguePanels();
                HideAllCharacters();

                notificationController.Show(node.notification, () =>
                {
                    Debug.Log("NODE MODAL branch entered");
                    MarkNotificationShown(node.notification);
                    ContinueShowNode(node, nodeId);
                });
                return;
            }

            if (mode == "toast")
            {
                notificationController.Show(node.notification);
                MarkNotificationShown(node.notification);
            }
        }

        ContinueShowNode(node, nodeId);
    }

    private void ContinueShowNode(DialogueNode node, string nodeId)
    {
        // Background
        if (backgroundController != null)
        {
            if (node.stopPreviousBgEffect) backgroundController.StopEffect();
            if (!string.IsNullOrEmpty(node.background)) backgroundController.ApplyBackground(node.background, node.bgFx);
            if (!string.IsNullOrEmpty(node.bgFx) && node.bgFx != "none") backgroundController.PlayEffect(node.bgFx);
        }

        ApplyNodeAudio(node);

        // Text + portraits
        if (IsNarrator(node.characterId))
        {
            ui.ShowAuthor(node.text);
            HideAllCharacters();
        }
        else if (IsLeftCharacter(node.characterId))
        {
            var meta = GetCharacterMeta(currentScene.leftCharacterId);

            ui.ShowLeftCharacter(meta?.displayName, node.text);
            ShowLeft(meta?.portraitKey, node.emotion);

            if (HideRightWhenLeftSpeaks) HideRight();
        }
        else if (IsRightAllowed(node.characterId))
        {
            var meta = GetCharacterMeta(node.characterId);

            ui.ShowRightCharacter(meta?.displayName, node.text);
            ShowRight(meta?.portraitKey, node.emotion);

            if (HideLeftWhenRightSpeaks) HideLeft();
            else if (!string.IsNullOrEmpty(currentScene?.leftCharacterId))
            {
                var leftMeta = GetCharacterMeta(currentScene.leftCharacterId);
                ShowLeft(leftMeta?.portraitKey, "Calm");
            }
        }
        else
        {
            var meta = GetCharacterMeta(node.characterId);
            ui.ShowRightCharacter(meta?.displayName ?? node.characterId, node.text);
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

        if (currentNode == null || currentNode.choices == null)
        {
            if (!string.IsNullOrEmpty(nextNodeId))
                ShowNode(nextNodeId);
            else
                ui.HideChoices();

            return;
        }

        Choice picked = currentNode.choices.Find(c => c != null && c.nextNode == nextNodeId);

        if (picked != null && picked.effects != null && picked.effects.Count > 0)
        {
            ApplyChoiceEffects(picked.effects);
            SaveSystem.Save(save);
        }

        if (picked != null && picked.notification != null && notificationController != null)
        {
            string mode = string.IsNullOrWhiteSpace(picked.notification.mode)
                ? ""
                : picked.notification.mode.Trim().ToLower();

            if (mode == "modal")
            {
                if (statFeedbackUI != null)
                {
                    statFeedbackUI.WaitUntilFinished(() =>
                    {
                        ui.HideChoices();
                        ui.HideDialoguePanels();
                        HideAllCharacters();

                        notificationController.Show(picked.notification, () =>
                        {
                            MarkNotificationShown(picked.notification);

                            if (!string.IsNullOrEmpty(nextNodeId))
                                ShowNode(nextNodeId);
                            else
                                ui.HideChoices();
                        });
                    });
                }
                else
                {
                    ui.HideChoices();
                    ui.HideDialoguePanels();
                    HideAllCharacters();

                    notificationController.Show(picked.notification, () =>
                    {
                        MarkNotificationShown(picked.notification);

                        if (!string.IsNullOrEmpty(nextNodeId))
                            ShowNode(nextNodeId);
                        else
                            ui.HideChoices();
                    });
                }

                return;
            }

            if (mode == "toast")
            {
                if (statFeedbackUI != null)
                {
                    statFeedbackUI.WaitUntilFinished(() =>
                    {
                        notificationController.Show(picked.notification);
                        MarkNotificationShown(picked.notification);
                    });
                }
                else
                {
                    notificationController.Show(picked.notification);
                    MarkNotificationShown(picked.notification);
                }
            }
        }

        if (!string.IsNullOrEmpty(nextNodeId))
            ShowNode(nextNodeId);
        else
            ui.HideChoices();
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

    bool ShouldShowNotification(NotificationData notification)
    {
        if (notification == null)
            return false;

        if (!notification.showOnce)
            return true;

        if (save == null)
            return true;

        if (save.shownNotificationIds == null)
            save.shownNotificationIds = new List<string>();

        if (string.IsNullOrEmpty(notification.id))
            return true;

        return !save.shownNotificationIds.Contains(notification.id);
    }

    void MarkNotificationShown(NotificationData notification)
    {
        if (notification == null || !notification.showOnce || string.IsNullOrEmpty(notification.id))
            return;

        if (save.shownNotificationIds == null)
            save.shownNotificationIds = new List<string>();

        if (!save.shownNotificationIds.Contains(notification.id))
        {
            save.shownNotificationIds.Add(notification.id);
            SaveSystem.Save(save);
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

    bool HasAction(DialogueNode node)
    {
        return node != null && !string.IsNullOrEmpty(node.action);
    }

    void HandleActionNode(DialogueNode node)
    {
        /*
        ACTION SYSTEM на будущее:

        action = это "что сделать"
        requirements = это "когда это разрешено"

        Использование:
        - action: вызывает особое поведение (не диалог)
            примеры:
                "summary_screen"
                "skip_scene"
                "open_panel"
                "play_cutscene"

        - requirements: проверяет условия (risk, trust, safety и т.д.)
            пример:
                risk >= 2

        Паттерн:
            если requirements выполнены → выполняем action
            если нет → игнорируем или идём по другой ветке

        Важно:
        НЕ зашивать условия внутрь action (типа "show_if_risk_2")
        Всю логику условий держать в requirements.

        Когда дойдём до ветвлений:
        - использовать action + requirements для:
            • пропуска сцен
            • скрытых веток
            • альтернативных концовок
        */

        if (node == null) return;

        switch (node.action)
        {
            case "summary_screen":
                ShowEpisodeEndPanel();
                break;
            case "open_note":
                UnlockNote(node.action);
                break;

            default:
                Debug.LogWarning($"[DialogueController] Unknown action: {node.action}");
                break;
        }
    }


    void ShowEpisodeEndPanel()
    {
        Debug.Log("[DialogueController] ShowEpisodeEndPanel called");

        if (episodeEndPanel == null)
        {
            Debug.LogError("[DialogueController] episodeEndPanel is NULL");
            return;
        }

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

        string nextEpisodePath = GetNextEpisodePath();
        string nextEpisodeStartNode = GetNextEpisodeStartNode();

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

        save.episodeStartSnapshot = new EpisodeSnapshot
        {
            sparks = save.sparksTotal,
            trustAG = save.trustAGTotal,
            trustJA = save.trustJATotal,
            risk = save.riskTotal,
            safety = save.safetyTotal
        };

        if (episodeEndPanel != null)
            episodeEndPanel.Hide();

        SaveSystem.Save(save);

        LoadEpisode();
        ShowNode(startNodeId);
    }

    // TODO: finish later: this will be used to unlock notes from dialogue nodes (node.effects can have "unlock_note:noteId")
    void UnlockNote(string noteId)
    {
        if (string.IsNullOrEmpty(noteId))
        {
            Debug.LogWarning("[DialogueController] UnlockNote called with empty noteId");
            return;
        }

        if (save == null)
        {
            Debug.LogError("[DialogueController] SaveData is null");
            return;
        }

        NoteState note = save.GetOrCreateNote(noteId);

        if (!note.isUnlocked)
        {
            note.isUnlocked = true;
            SaveSystem.Save(save);
            Debug.Log("[DialogueController] Note unlocked: " + noteId);
        }
        else
        {
            Debug.Log("[DialogueController] Note already unlocked: " + noteId);
        }
    }
    
    private void ApplyNodeAudio(DialogueNode node)
    {
        if (node == null || node.audio == null)
            return;

        if (SceneMusicController.Instance == null)
            return;

        SceneMusicController.Instance.ApplyNodeAudio(node.audio);
    }
}