using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dialogue controller: EpisodeData -> scenes[] -> nodes[]
///
/// Rules:
/// - character == Narrator/Автор/empty  => hide both portraits
/// - emotion == null/empty and NOT narrator => use "Calm"
/// - left character is fixed per scene (scene.leftCharacter)
/// - right characters are allowed per scene (scene.rightCharacters)
///
/// Portrait rules:
/// - Left: exactly one per scene (can change between scenes, unlimited across episode)
/// - Right: can change many times inside a scene (unlimited across episode)
/// </summary>
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
    [Tooltip("If true: when right character speaks, left portrait is hidden.")]
    public bool HideLeftWhenRightSpeaks = true;

    [Tooltip("If true: when left character speaks, right portrait is hidden.")]
    public bool HideRightWhenLeftSpeaks = true;

    [Header("Layout")]
    public LayoutController layout;

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

        if (nodeDict == null || nodeDict.Count == 0)
        {
            Debug.LogError("[DialogueController] nodeDict empty");
            return;
        }

        ShowNode(startNodeId);
    }

    void Update()
    {
        if (waitingForAdvance && Input.GetMouseButtonDown(0))
        {
            waitingForAdvance = false;

            if (currentNode == null) return;

            // Ending node: has requirements & no next
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

    bool IsNarrator(string ch)
    {
        return string.IsNullOrEmpty(ch) || ch == "Narrator" || ch == "Автор";
    }

    string NormalizeEmotion(string em)
    {
        return string.IsNullOrEmpty(em) ? "Calm" : em;
    }

    void HideLeft()
    {
        if (LeftCharacter != null) LeftCharacter.SetActive(false);
        // optional: LeftPortrait?.Hide();
    }

    void ShowLeft(string characterName, string emotion)
    {
        if (LeftCharacter != null) LeftCharacter.SetActive(true);
        LeftPortrait?.Show(characterName, NormalizeEmotion(emotion));
    }

    void HideRight()
    {
        if (RightCharacter != null) RightCharacter.SetActive(false);
        // optional: RightPortrait?.Hide();
    }

    void ShowRight(string characterName, string emotion)
    {
        if (RightCharacter != null) RightCharacter.SetActive(true);
        RightPortrait?.Show(characterName, NormalizeEmotion(emotion));
    }

    void HideAllCharacters()
    {
        HideLeft();
        HideRight();
    }

    void RebuildRightAllowed(SceneData scene)
    {
        currentRightAllowed.Clear();
        if (scene == null || scene.rightCharacters == null) return;

        foreach (var c in scene.rightCharacters)
            if (!string.IsNullOrEmpty(c))
                currentRightAllowed.Add(c);
    }

    void EnterSceneIfChanged(string nodeId)
    {
        if (nodeToScene == null || !nodeToScene.TryGetValue(nodeId, out var sc) || sc == null)
            return;

        bool changed = (currentScene == null) || (currentScene.sceneId != sc.sceneId);
        if (!changed) return;

        currentScene = sc;
        RebuildRightAllowed(currentScene);

        // Scene-level background on enter (node.background may override later)
        if (backgroundController != null && !string.IsNullOrEmpty(currentScene.background))
            backgroundController.SetBackground(currentScene.background);

        // Left: fixed character per scene
        if (!string.IsNullOrEmpty(currentScene.leftCharacter))
            ShowLeft(currentScene.leftCharacter, "Calm");
        else
            HideLeft();

        // Right: start hidden (appears when a right character speaks)
        HideRight();
    }

    bool IsLeftCharacter(string ch)
    {
        return currentScene != null &&
               !string.IsNullOrEmpty(currentScene.leftCharacter) &&
               ch == currentScene.leftCharacter;
    }

    bool IsRightAllowed(string ch)
    {
        return !string.IsNullOrEmpty(ch) && currentRightAllowed.Contains(ch);
    }

    // ---------------- Main ----------------
    void ShowNode(string nodeId)
    {
        if (nodeDict == null || !nodeDict.TryGetValue(nodeId, out var node) || node == null)
        {
            Debug.LogError("[DialogueController] Node not found: " + nodeId);
            return;
        }

        // Ensure scene context is correct
        EnterSceneIfChanged(nodeId);

        currentNode = node;
        waitingForAdvance = false;

        // Apply node effects once
        ApplyEffectsOnce(nodeId, node);

        // Background override by node
        if (backgroundController != null && !string.IsNullOrEmpty(node.background))
            backgroundController.SetBackground(node.background);

        // -------- Text + portraits --------
        if (IsNarrator(node.character))
        {
            ui.ShowAuthor(node.text);
            if (ui.authorText != null) ui.authorText.color = AuthorColor;

            HideAllCharacters();
        }
        else if (IsLeftCharacter(node.character))
        {
            ui.ShowLeftCharacter(node.character, node.text);
            if (ui.LeftCharacterText != null) ui.LeftCharacterText.color = LeftCharacterColor;

            // Left speaks (left character name is defined by the scene)
            if (!string.IsNullOrEmpty(currentScene?.leftCharacter))
                ShowLeft(currentScene.leftCharacter, node.emotion);

            if (HideRightWhenLeftSpeaks)
                HideRight();
        }
        else if (IsRightAllowed(node.character))
        {
            ui.ShowOther(node.character, node.text);
            if (ui.otherText != null) ui.otherText.color = OtherColor;

            // Right speaks (node.character is the right speaker)
            ShowRight(node.character, node.emotion);

            if (HideLeftWhenRightSpeaks)
                HideLeft();
            else
            {
                // Keep left visible if the scene defines one
                if (!string.IsNullOrEmpty(currentScene?.leftCharacter))
                    if (LeftCharacter != null) LeftCharacter.SetActive(true);
            }
        }
        else
        {
            // Unknown/system/data mistake
            ui.ShowOther(node.character, node.text);
            if (ui.otherText != null) ui.otherText.color = OtherColor;

            HideAllCharacters();
        }

        // Choices / click advance
        SetupChoices(node);

        // Layout
        if (layout != null)
        {
            string ch = IsNarrator(node.character) ? "Narrator" : node.character;
            layout.ApplyLayout(ch);
        }

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

        if (!string.IsNullOrEmpty(nextNodeId))
            ShowNode(nextNodeId);
        else
            ui.HideChoices();
    }

    void TryApplyPickedChoiceEffects(string nextNodeId)
    {
        if (currentNode == null || currentNode.choices == null || string.IsNullOrEmpty(nextNodeId))
            return;

        Choice picked = null;
        for (int i = 0; i < currentNode.choices.Count; i++)
        {
            var c = currentNode.choices[i];
            if (c != null && c.nextNode == nextNodeId)
            {
                picked = c;
                break;
            }
        }

        if (picked == null || picked.effects == null || picked.effects.Count == 0)
            return;

        ApplyChoiceEffects(picked.effects);
        SaveSystem.Save(save);
    }

    // -------- Effects (OLD node.effects kept) --------

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

    // -------- Choice effects (NEW ops list) --------
    void ApplyChoiceEffects(List<EffectOp> ops)
    {
        if (ops == null) return;

        foreach (var op in ops)
        {
            if (op == null) continue;
            if (string.IsNullOrEmpty(op.op) || string.IsNullOrEmpty(op.key)) continue;
            if (op.op != "inc") continue;

            int v = op.value;

            switch (op.key)
            {
                case "risk":
                    save.riskTotal += v;
                    save.episodeRisk += v;
                    break;

                case "safety":
                    save.safetyTotal += v;
                    save.episodeSafety += v;
                    break;

                case "trust.Ainaz_Guldana":
                case "trust.Ainaz_Guldana ":
                    save.trustAG += v;
                    break;
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