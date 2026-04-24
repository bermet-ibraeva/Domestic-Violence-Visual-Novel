using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
        save = SaveSystem.Load() ?? new SaveData();

        NormalizeSave();

        if (string.IsNullOrEmpty(save.episodePath))
        {
            Debug.LogError("[DialogueController] No episodePath in save!");
            return;
        }

        episode = EpisodeLoader.LoadEpisode(
            save.episodePath,
            out nodeDict,
            out sceneDict,
            out nodeToScene
        );

        if (episode == null)
        {
            Debug.LogError($"[DialogueController] Failed to load episode: {save.episodePath}");
            return;
        }

        BuildCharacterMetaDict();

        StatSystem.Instance.Init(save);

        episodeEndPanel?.Init(this);
        episodeEndPanel?.Hide();

        string startNode = null;

        foreach (var scene in episode.scenes)
        {
            if (!string.IsNullOrEmpty(scene.startNode))
            {
                startNode = scene.startNode;
                break;
            }
        }

        if (string.IsNullOrEmpty(startNode))
        {
            Debug.LogError("[DialogueController] Cannot determine start node.");
            return;
        }

        if (string.IsNullOrEmpty(save.currentNodeId))
        {
            save.currentNodeId = startNode;

            save.episodeStartSnapshot = new EpisodeSnapshot
            {
                sparks = save.sparksTotal,
                trustAG = save.trustAGTotal,
                trustJA = save.trustJATotal,
                risk = save.riskTotal,
                safety = save.safetyTotal
            };

            SaveSystem.Save(save);
        }

        ShowNode(save.currentNodeId);
    }

    void NormalizeSave()
    {
        if (save.appliedEffectNodes == null)
            save.appliedEffectNodes = new List<string>();

        if (save.shownNotificationIds == null)
            save.shownNotificationIds = new List<string>();
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
        episode = EpisodeLoader.LoadEpisode(save.episodePath, out nodeDict, out sceneDict, out nodeToScene);

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
            if (node.action.type == "summary_screen")
            {
                HandleActionNode(node);
                return; // only this one stops flow
            }

            HandleActionNode(node);
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
        save.currentNodeId = nodeId;
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
        string key = $"{save.episodePath}:{nodeId}";
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

    bool HasAction(DialogueNode node)
    {
        return node != null && node.action != null && !string.IsNullOrEmpty(node.action.type);
    }

    
    void HandleActionNode(DialogueNode node)
    {
        if (node == null || node.action == null)
            return;

        switch (node.action.type)
        {
            case "summary_screen":
                ShowEpisodeEndPanel();
                break;

            case "unlock_note":
                HandleUnlockNote(node.action);
                break;

            default:
                Debug.LogWarning($"[DialogueController] Unknown action type: {node.action.type}");
                break;
        }
    }

    void HandleUnlockNote(NodeAction action)
    {
        if (action == null || string.IsNullOrEmpty(action.noteId))
        {
            Debug.LogWarning("[DialogueController] unlock_note: noteId missing");
            return;
        }

        if (action.status == "unlocked")
        {
            UnlockNote(action.noteId);
        }
        else
        {
            Debug.LogWarning($"[DialogueController] Unknown note status: {action.status}");
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
        if (string.IsNullOrEmpty(save.episodePath))
            return null;

        string fileName = save.episodePath.Substring(save.episodePath.LastIndexOf('_') + 1);

        if (!int.TryParse(fileName, out int currentEpisodeNumber))
        {
            Debug.LogWarning("[DialogueController] Could not parse current episode number from path: " + save.episodePath);
            return null;
        }

        int nextEpisodeNumber = currentEpisodeNumber + 1;
        return $"E{nextEpisodeNumber:D2}_S01_start";
    }

    string GetNextEpisodePath()
    {
        if (string.IsNullOrEmpty(save.episodePath))
            return null;

        int lastSlash = save.episodePath.LastIndexOf('/');
        int lastUnderscore = save.episodePath.LastIndexOf('_');

        if (lastSlash < 0 || lastUnderscore < 0 || lastUnderscore <= lastSlash)
            return null;

        string prefix = save.episodePath.Substring(0, lastUnderscore + 1); // "Demos/demo_"
        string numberPart = save.episodePath.Substring(lastUnderscore + 1);

        if (!int.TryParse(numberPart, out int currentNumber))
            return null;

        int nextNumber = currentNumber + 1;

        return prefix + nextNumber;
    }

    public void StartNextEpisode(string newEpisodePath)
    {
        SaveSystem.StartEpisode(newEpisodePath);

        SceneManager.LoadScene("EpisodePage");
    }

    int ExtractEpisodeNumber(string path)
    {
        if (string.IsNullOrEmpty(path))
            return 1;

        string fileName = path.Substring(path.LastIndexOf('_') + 1);

        if (int.TryParse(fileName, out int number))
            return number;

        Debug.LogWarning($"[DialogueController] Failed to extract episode number from path: {path}");
        return 1;
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
            save.UnlockNote(noteId);
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

        if (SceneMusicManager.Instance == null)
            return;

        SceneMusicManager.Instance.ApplyNodeAudio(node.audio);
    }
}