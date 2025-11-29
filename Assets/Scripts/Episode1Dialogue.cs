using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

#region JSON STRUCTURES

[System.Serializable]
public class EpisodeData
{
    public string episode;
    public Variables variables;
    public List<DialogueNode> nodes;
}

[System.Serializable]
public class Variables
{
    public int Сострадание;
    public int Послушание;
    public int Сопротивление;
    public int Тревога;
    public int Доверие;
}

[System.Serializable]
public class DialogueNode
{
    public string nodeId;
    public string background;
    public string character;
    public string emotion;
    public string text;
    public List<Choice> choices;
    public Effects effects;
    public string nextNode;
    public Requirement[] requirements;
}

[System.Serializable]
public class Choice
{
    public string text;
    public string nextNode;
}

[System.Serializable]
public class Effects
{
    public int Сострадание;
    public int Послушание;
    public int Сопротивление;
    public int Тревога;
    public int Доверие;
}

[System.Serializable]
public class Requirement
{
    public string condition;
    public string ending;
}

#endregion

public class Episode1Dialogue : MonoBehaviour
{
    [Header("Author UI (центр, без имени)")]
    public GameObject AuthorPanel;
    public TextMeshProUGUI AuthorText;

    [Header("Ainaz UI (центр, форма для Айназ)")]
    public GameObject AinazPanel;
    public TextMeshProUGUI AinazNameText;
    public TextMeshProUGUI AinazDialogueText;

    [Header("Other Character UI (центр, форма для других)")]
    public GameObject OtherPanel;
    public TextMeshProUGUI OtherNameText;
    public TextMeshProUGUI OtherDialogueText;

    [Header("Choices")]
    public GameObject ChoicesPanel;
    public Button[] choiceButtons;

    [Header("Characters Sprites")]
    public GameObject LeftCharacter;          // Айназ слева
    public EmotionsController LeftEmotions;

    public GameObject RightCharacter;         // любые другие справа
    public EmotionsController RightEmotions;

    [Header("Text Colors")]
    public Color AuthorColor = Color.gray;
    public Color AinazColor = Color.white;
    public Color OtherColor = Color.white;

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
        ShowNode(startNodeId);
    }

    void Update()
    {
        // Клик ЛКМ — переход, если нет выборов
        if (waitingForAdvance && Input.GetMouseButtonDown(0))
        {
            waitingForAdvance = false;

            if (currentNode == null)
                return;

            // Если есть requirements — считаем концовку
            if (currentNode.requirements != null && currentNode.requirements.Length > 0)
            {
                HandleEnding(currentNode);
                return;
            }

            // Переход по nextNode
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

    // =================== LOAD JSON ===================
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

    // =================== SHOW NODE ===================
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

        ApplyEffects(node);

        // Сначала всё прячем
        if (AuthorPanel != null) AuthorPanel.SetActive(false);
        if (AinazPanel  != null) AinazPanel.SetActive(false);
        if (OtherPanel  != null) OtherPanel.SetActive(false);
        if (LeftCharacter  != null) LeftCharacter.SetActive(false);
        if (RightCharacter != null) RightCharacter.SetActive(false);

        // Выбираем режим по character
        if (node.character == "Автор" || string.IsNullOrEmpty(node.character))
        {
            // Авторские мысли / описания
            if (AuthorPanel != null) AuthorPanel.SetActive(true);
            if (AuthorText != null)
            {
                AuthorText.color = AuthorColor;
                AuthorText.text = node.text;
            }
        }
        else if (node.character == "Айназ")
        {
            // Айназ: панель для Айназ, персонаж слева
            if (AinazPanel != null) AinazPanel.SetActive(true);
            if (LeftCharacter != null) LeftCharacter.SetActive(true);

            if (AinazNameText != null)
                AinazNameText.text = node.character;

            if (AinazDialogueText != null)
            {
                AinazDialogueText.color = AinazColor;
                AinazDialogueText.text = node.text;
            }

            ApplyEmotion(LeftEmotions, node.emotion);
        }
        else
        {
            // Любой другой персонаж: панель для других, персонаж справа
            if (OtherPanel != null) OtherPanel.SetActive(true);
            if (RightCharacter != null) RightCharacter.SetActive(true);

            if (OtherNameText != null)
                OtherNameText.text = node.character;

            if (OtherDialogueText != null)
            {
                OtherDialogueText.color = OtherColor;
                OtherDialogueText.text = node.text;
            }

            ApplyEmotion(RightEmotions, node.emotion);
        }

        // TODO: тут можно добавить смену фона по node.background

        // Настраиваем выборы
        SetupChoices(node);
    }

    // =================== APPLY EMOTION ===================
    void ApplyEmotion(EmotionsController controller, string emotion)
    {
        if (controller == null) return;

        switch (emotion)
        {
            case "Calm":   controller.SetCalm();   break;
            case "Sad":    controller.SetSad();    break;
            case "Scared": controller.SetScared(); break;
            case "Happy":  controller.SetHappy();  break;
            default:       controller.SetCalm();   break;
        }
    }

    // =================== CHOICE BUTTONS ===================
    void SetupChoices(DialogueNode node)
    {
        bool hasChoices = node.choices != null && node.choices.Count > 0;

        if (ChoicesPanel != null)
            ChoicesPanel.SetActive(hasChoices);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            Button btn = choiceButtons[i];
            if (btn == null) continue;

            if (hasChoices && i < node.choices.Count)
            {
                btn.gameObject.SetActive(true);

                TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = node.choices[i].text;

                string targetNodeId = node.choices[i].nextNode;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    ShowNode(targetNodeId);
                });

                // Гарантия, что есть Image для Raycast
                if (btn.GetComponent<Image>() == null)
                {
                    Image img = btn.gameObject.AddComponent<Image>();
                    img.color = new Color(1, 1, 1, 0);
                }
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }

        // если нет вариантов — ждём клик по экрану
        if (!hasChoices)
            waitingForAdvance = true;
    }

    // =================== STATS EFFECTS ===================
    void ApplyEffects(DialogueNode node)
    {
        if (node.effects == null) return;

        vars.Сострадание   += node.effects.Сострадание;
        vars.Послушание    += node.effects.Послушание;
        vars.Сопротивление += node.effects.Сопротивление;
        vars.Тревога       += node.effects.Тревога;
        vars.Доверие       += node.effects.Доверие;
    }

    // =================== ENDINGS ===================
    void HandleEnding(DialogueNode node)
    {
        if (node.requirements == null || node.requirements.Length == 0)
        {
            Debug.Log("ENDING: no requirements configured");
            return;
        }

        if (vars.Сопротивление >= vars.Послушание)
        {
            Debug.Log("GOOD ENDING: " + node.requirements[0].ending);
        }
        else
        {
            if (node.requirements.Length > 1)
                Debug.Log("SILENT ENDING: " + node.requirements[1].ending);
            else
                Debug.Log("ENDING: " + node.requirements[0].ending);
        }

        if (ChoicesPanel != null)
            ChoicesPanel.SetActive(false);
        // Тут потом можно открыть финальный экран
    }
}
