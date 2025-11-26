using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class DialogueOption
{
    public string text;
    public int nextIndex;
}

[System.Serializable]
public class DialogueEntry
{
    public string character;
    public string text;
    public string emotion;
    public List<DialogueOption> options;
}

[System.Serializable]
public class DialogueList
{
    public DialogueEntry[] items;
}

public class Episode1Dialogue : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject AuthorDialoguePanel;      // Панель автора (Image + TextMeshPro)
    public TextMeshProUGUI AuthorCommentText;

    public GameObject AinazDialoguePanel;       // Панель Айназ (Image + TextMeshPro)
    public TextMeshProUGUI AinazDialogueText;
    public TextMeshProUGUI AinazNameText;       // Имя сверху панели Айназ

    public Button[] choiceButtons;

    [Header("Character Controller")]
    public EmotionsController characterController;

    [Header("Text Colors")]
    public Color AuthorTextColor = Color.gray;
    public Color AinazTextColor = Color.white;

    private List<DialogueEntry> dialogueEntries;
    private int currentIndex = 0;

    void Start()
    {
        LoadDialogue();
        StartCoroutine(PlayDialogue(currentIndex));
    }

    void LoadDialogue()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "episode1.json");
        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found at " + path);
            return;
        }

        string jsonString = File.ReadAllText(path);
        DialogueList listWrapper = JsonUtility.FromJson<DialogueList>("{\"items\":" + jsonString + "}");
        dialogueEntries = new List<DialogueEntry>(listWrapper.items);
    }

    IEnumerator PlayDialogue(int index)
    {
        if (index >= dialogueEntries.Count) yield break;

        DialogueEntry entry = dialogueEntries[index];

        // Выбираем панель и текст для текущего персонажа
        GameObject currentPanel;
        GameObject otherPanel;
        TextMeshProUGUI currentTextUI;

        if (entry.character == "Айназ")
        {
            currentPanel = AinazDialoguePanel;
            otherPanel = AuthorDialoguePanel;
            currentTextUI = AinazDialogueText;
            currentTextUI.color = AinazTextColor;

            // Имя сверху панели Айназ
            AinazNameText.text = entry.character;
        }
        else
        {
            currentPanel = AuthorDialoguePanel;
            otherPanel = AinazDialoguePanel;
            currentTextUI = AuthorCommentText;
            currentTextUI.color = AuthorTextColor;
        }

        currentPanel.SetActive(true);
        otherPanel.SetActive(false);

        SetPanelPosition(currentPanel.GetComponent<RectTransform>(), entry.character);

        // Эмоции только для Айназ
        if (entry.character == "Айназ")
        {
            switch (entry.emotion)
            {
                case "Calm": characterController.SetCalm(); break;
                case "Sad": characterController.SetSad(); break;
                case "Scared": characterController.SetScared(); break;
                case "Happy": characterController.SetHappy(); break;
                default: characterController.SetCalm(); break;
            }
        }

        // Настройка кнопок выбора
        bool hasOptions = entry.options != null && entry.options.Count > 0;
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (hasOptions && i < entry.options.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = entry.options[i].text;
                int nextIndex = entry.options[i].nextIndex;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() =>
                {
                    StopAllCoroutines();
                    StartCoroutine(PlayDialogue(nextIndex));
                });
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        // Эффект печатающего текста
        yield return StartCoroutine(TypewriterEffect(currentTextUI, entry.text));

        // Если нет выбора, ждем рассчитанное время и продолжаем
        if (!hasOptions)
        {
            float duration = CalculateDuration(entry.text, entry.emotion);
            yield return new WaitForSeconds(duration);
            currentIndex++;
            StartCoroutine(PlayDialogue(currentIndex));
        }
    }

    IEnumerator TypewriterEffect(TextMeshProUGUI textUI, string fullText, float charDelay = 0.02f)
    {
        textUI.text = "";
        foreach (char c in fullText)
        {
            textUI.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }

    float CalculateDuration(string text, string emotion)
    {
        int wordCount = text.Split(' ').Length;
        float baseTime = wordCount * 0.5f;

        switch (emotion)
        {
            case "Scared": baseTime *= 1.2f; break;
            case "Sad": baseTime *= 1.1f; break;
            case "Happy": baseTime *= 1f; break;
            case "Calm": baseTime *= 1f; break;
        }

        return Mathf.Clamp(baseTime, 2f, 12f);
    }

    void SetPanelPosition(RectTransform rect, string character)
    {
        if (character == "Айназ")
        {
            rect.anchorMin = new Vector2(0.7f, 0.3f); 
            rect.anchorMax = new Vector2(0.95f, 0.6f);
        }
        else
        {
            rect.anchorMin = new Vector2(0.3f, 0.3f);
            rect.anchorMax = new Vector2(0.7f, 0.6f);
        }
        rect.anchoredPosition = Vector2.zero;
    }
}
