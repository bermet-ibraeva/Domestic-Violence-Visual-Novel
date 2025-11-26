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
private class DialogueList
{
    public DialogueEntry[] items;
}

public class Episode1Dialogue : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI mainDialogueText;      // Для автора
    public TextMeshProUGUI aynazDialogueText;     // Для мыслей Айназ
    public TextMeshProUGUI characterText;         // Имя говорящего
    public Button[] choiceButtons;

    [Header("Character Controller")]
    public EmotionsController characterController;

    [Header("Text Colors")]
    public Color authorTextColor = Color.gray;
    public Color aynazTextColor = Color.white;

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

        // Устанавливаем имя говорящего
        characterText.text = entry.character;

        // Выбираем TextMeshPro для текущего текста
        TextMeshProUGUI currentTextUI;
        TextMeshProUGUI otherTextUI;

        if (entry.character == "Айназ")
        {
            currentTextUI = aynazDialogueText;
            otherTextUI = mainDialogueText;
            currentTextUI.color = aynazTextColor;
        }
        else
        {
            currentTextUI = mainDialogueText;
            otherTextUI = aynazDialogueText;
            currentTextUI.color = authorTextColor;
        }

        currentTextUI.gameObject.SetActive(true);
        otherTextUI.gameObject.SetActive(false);

        // Устанавливаем позицию текста
        SetTextPosition(currentTextUI, entry.character);

        // Устанавливаем эмоцию только для Айназ
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

        // Печатаем текст с эффектом печати
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

    // Эффект печатающего текста
    IEnumerator TypewriterEffect(TextMeshProUGUI textUI, string fullText, float charDelay = 0.02f)
    {
        textUI.text = "";
        foreach (char c in fullText)
        {
            textUI.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }

    // Расчет времени текста по словам и эмоции
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

    // Установка позиции текста
    void SetTextPosition(TextMeshProUGUI textUI, string character)
    {
        RectTransform rect = textUI.GetComponent<RectTransform>();

        if (character == "Айназ")
        {
            rect.anchorMin = new Vector2(0.7f, 0.3f); // справа
            rect.anchorMax = new Vector2(0.95f, 0.6f);
        }
        else
        {
            rect.anchorMin = new Vector2(0.3f, 0.3f); // по центру
            rect.anchorMax = new Vector2(0.7f, 0.6f);
        }

        rect.anchoredPosition = Vector2.zero;
    }
}
