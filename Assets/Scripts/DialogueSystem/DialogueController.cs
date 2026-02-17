using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    public UIController ui;

    [Header("Characters")]
    public GameObject LeftCharacter;
    public EmotionsController LeftEmotions;

    public GameObject RightCharacter;
    public RightCharacterController RightCharacterCtrl;

    [Header("Layout")]
    public LayoutController layout;

    [Header("Text Colors")]
    public Color AuthorColor = Color.gray;
    public Color AinazColor = Color.white;
    public Color OtherColor = Color.white;

    [Header("Backgrounds")]
    public BackgroundController backgroundController;

    [Header("Episode Settings")]
    public string episodePath = "Episodes/episode_1";
    public string startNodeId = "scene_1_start";

    [Header("Chapter")]
    public int chapterNumber = 1;

    private EpisodeData episode;
    private Dictionary<string, DialogueNode> nodeDict;

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
            Debug.LogError("DialogueController: nodeDict empty");
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
        episode = EpisodeLoader.LoadEpisode(episodePath, out nodeDict);
        if (episode == null)
            Debug.LogError("DialogueController: Episode failed to load");
    }

    void ShowNode(string nodeId)
    {
        if (nodeDict == null || !nodeDict.TryGetValue(nodeId, out var node))
        {
            Debug.LogError("Node not found: " + nodeId);
            return;
        }

        currentNode = node;
        waitingForAdvance = false;

        // ✅ Apply effects once
        ApplyEffectsOnce(nodeId, node);

        // Background
        if (backgroundController != null && !string.IsNullOrEmpty(node.background))
            backgroundController.SetBackground(node.background);

        // Hide characters
        if (LeftCharacter != null) LeftCharacter.SetActive(false);
        if (RightCharacter != null) RightCharacter.SetActive(false);

        // Text + sprites
        if (node.character == "Автор" || string.IsNullOrEmpty(node.character))
        {
            ui.ShowAuthor(node.text);
            ui.authorText.color = AuthorColor;
        }
        else if (node.character == "Айназ")
        {
            ui.ShowAinaz(node.character, node.text);
            ui.ainazText.color = AinazColor;

            if (LeftCharacter != null) LeftCharacter.SetActive(true);
            LeftEmotions?.SetEmotion(node.emotion);
        }
        else
        {
            ui.ShowOther(node.character, node.text);
            ui.otherText.color = OtherColor;

            if (RightCharacter != null) RightCharacter.SetActive(true);
            RightCharacterCtrl?.Show(node.character, node.emotion);
        }

        SetupChoices(node);

        if (layout != null)
        {
            string ch = string.IsNullOrEmpty(node.character) ? "Автор" : node.character;
            layout.ApplyLayout(ch);
        }

        // Autosave pointers
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

        if (!string.IsNullOrEmpty(nextNodeId))
            ShowNode(nextNodeId);
        else
            ui.HideChoices();
    }

    // -------- Effects (NEW) --------
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

    void HandleEnding(DialogueNode node)
    {
        // пока просто выводим ending текста (логику условий добавим потом)
        if (node.requirements == null || node.requirements.Length == 0)
        {
            ui.HideChoices();
            return;
        }

        Debug.Log("ENDING: " + node.requirements[0].ending);
        ui.HideChoices();
    }
}