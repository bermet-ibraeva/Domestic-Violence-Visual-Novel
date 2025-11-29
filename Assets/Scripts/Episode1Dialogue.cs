using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class Episode1Dialogue : MonoBehaviour
{
    [Header("Author UI (центр, без имени)")]
    public GameObject AuthorPanel;
    public TextMeshProUGUI AuthorText;

    [Header("Ainaz UI")]
    public GameObject AinazPanel;
    public TextMeshProUGUI AinazNameText;
    public TextMeshProUGUI AinazDialogueText;

    [Header("Other Character UI")]
    public GameObject OtherPanel;
    public TextMeshProUGUI OtherNameText;
    public TextMeshProUGUI OtherDialogueText;

    [Header("Choices")]
    public GameObject ChoicesPanel;
    public Button[] choiceButtons;

    [Header("Characters")]
    public GameObject LeftCharacter;          // Айназ
    public EmotionsController LeftEmotions;

    public GameObject RightCharacter;        // другие персонажи
    public EmotionsController RightEmotions;

    [Header("Text Colors")]
    public Color AuthorColor = Color.gray;
    public Color AinazColor = Color.white;
    public Color OtherColor = Color.white;

    [Header("Backgrounds")]
    public BackgroundController backgroundController;

    [Header("Episode")]
    public string jsonFileName = "episode1.json";
    public string startNodeId = "scene_1_start";

    private EpisodeData episode;
    private Variables vars;
    private Dictionary<string, DialogueNode> nodeDict;

    private DialogueNode currentNode;
    private bool waitingForAdvance = false;


    void Start()
    {
        LoadEpisode();

        if (nodeDict == null || nodeDict.Count == 0)
        {
            Debug.LogError("Episode1Dialogue: nodeDict is empty, cannot start dialogue");
            return;
        }

        ShowNode(startNodeId);
    }

    void Update()
    {
        if (waitingForAdvance && Input.GetMouseButtonDown(0))
        {
            waitingForAdvance = false;

            if (currentNode == null)
                return;

            if (currentNode.requirements != null && currentNode.requirements.Length > 0)
            {
                HandleEnding(currentNode);
                return;
            }

            if (!string.IsNullOrEmpty(currentNode.nextNode))
            {
                ShowNode(currentNode.nextNode);
            }
            else
            {
                ChoicesPanel.SetActive(false);
            }
        }
    }

    // ================= LOAD EPISODE =================
    void LoadEpisode()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found at " + path);
            return;
        }

        string json = File.ReadAllText(path);
        episode = JsonUtility.FromJson<EpisodeData>(json);

        if (episode == null)
        {
            Debug.LogError("Failed to parse JSON");
            return;
        }

        vars = episode.variables ?? new Variables();
        nodeDict = new Dictionary<string, DialogueNode>();

        foreach (var n in episode.nodes)
        {
            if (!nodeDict.ContainsKey(n.nodeId))
                nodeDict.Add(n.nodeId, n);
        }
    }

    // ================= SHOW NODE =================
    void ShowNode(string nodeId)
    {
        if (!nodeDict.ContainsKey(nodeId))
        {
            Debug.LogError("Node not found: " + nodeId);
            return;
        }

        DialogueNode node = nodeDict[nodeId];
        currentNode = node;
        waitingForAdvance = false;

        Debug.Log($"[DIALOGUE] ShowNode: {nodeId}, background = {node.background}");

        ApplyEffects(node);

        // ---------- ФОН ----------
        if (backgroundController != null && !string.IsNullOrEmpty(node.background))
        {
            backgroundController.SetBackground(node.background);
        }

        // Скрываем всё
        AuthorPanel?.SetActive(false);
        AinazPanel?.SetActive(false);
        OtherPanel?.SetActive(false);
        LeftCharacter?.SetActive(false);
        RightCharacter?.SetActive(false);

        // ---------- АВТОР ----------
        if (node.character == "Автор" || string.IsNullOrEmpty(node.character))
        {
            AuthorPanel.SetActive(true);
            AuthorText.color = AuthorColor;
            AuthorText.text = node.text;

            var auto = AuthorPanel.GetComponent<AutoResizePanel>();
            if (auto != null) auto.RefreshSize();
        }
        // ---------- АЙНАЗ ----------
        else if (node.character == "Айназ")
        {
            AinazPanel.SetActive(true);
            LeftCharacter.SetActive(true);

            AinazNameText.text = node.character;
            AinazDialogueText.color = AinazColor;
            AinazDialogueText.text = node.text;

            var auto = AinazPanel.GetComponent<AutoResizePanel>();
            if (auto != null) auto.RefreshSize();

            ApplyEmotion(LeftEmotions, node.emotion);
        }
        // ---------- ДРУГОЙ ПЕРСОНАЖ ----------
        else
        {
            OtherPanel.SetActive(true);
            RightCharacter.SetActive(true);

            OtherNameText.text = node.character;
            OtherDialogueText.color = OtherColor;
            OtherDialogueText.text = node.text;

            var auto = OtherPanel.GetComponent<AutoResizePanel>();
            if (auto != null) auto.RefreshSize();

            ApplyEmotion(RightEmotions, node.emotion);
        }

        SetupChoices(node);
    }


    // ================= EMOTIONS =================
    void ApplyEmotion(EmotionsController controller, string emotion)
    {
        if (controller == null)
        {
            Debug.LogWarning("ApplyEmotion: controller is NULL");
            return;
        }

        Debug.Log("ApplyEmotion: " + emotion);

        switch (emotion)
        {
            case "Calm":
                controller.SetCalm();
                break;
            case "Sad":
                controller.SetSad();
                break;
            case "Scared":
                controller.SetScared();
                break;
            case "Happy":
                controller.SetHappy();
                break;
            default:
                controller.SetCalm();
                break;
        }
    }


    // ================= CHOICES =================
    void SetupChoices(DialogueNode node)
    {
        bool hasChoices = node.choices != null && node.choices.Count > 0;

        ChoicesPanel.SetActive(hasChoices);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            Button btn = choiceButtons[i];

            if (hasChoices && i < node.choices.Count)
            {
                btn.gameObject.SetActive(true);

                var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                btnText.text = node.choices[i].text;

                string next = node.choices[i].nextNode;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowNode(next));
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }

        if (!hasChoices)
            waitingForAdvance = true;
    }

    // ================= EFFECTS =================
    void ApplyEffects(DialogueNode node)
    {
        if (node.effects == null) return;

        vars.Сострадание += node.effects.Сострадание;
        vars.Послушание += node.effects.Послушание;
        vars.Сопротивление += node.effects.Сопротивление;
        vars.Тревога += node.effects.Тревога;
        vars.Доверие += node.effects.Доверие;
    }

    // ================= ENDINGS =================
    void HandleEnding(DialogueNode node)
    {
        if (vars.Сопротивление >= vars.Послушание)
            Debug.Log("GOOD ENDING: " + node.requirements[0].ending);
        else
            Debug.Log("SILENT ENDING: " + node.requirements[1].ending);

        ChoicesPanel.SetActive(false);
    }
}
