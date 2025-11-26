using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class Choice
{
    public string text;
    public int next;
}

[System.Serializable]
public class DialogueLine
{
    public string character;
    public string text;
    public Choice[] choices;
}

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI characterText;
    public TextMeshProUGUI dialogueText;
    public Button[] choiceButtons;

    private List<DialogueLine> lines;
    private int currentLine = 0;

    void Start()
    {
        LoadDialogue("Assets/Dialogs/Episode1.json");
        ShowLine();
    }

    void LoadDialogue(string path)
    {
        string json = File.ReadAllText(path);
        lines = new List<DialogueLine>(JsonUtility.FromJson<DialogueLineList>("{\"lines\":" + json + "}").lines);
    }

    void ShowLine()
    {
        if (currentLine >= lines.Count) return;

        DialogueLine line = lines[currentLine];
        characterText.text = line.character;
        dialogueText.text = line.text;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < line.choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = line.choices[i].text;
                int next = line.choices[i].next;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() =>
                {
                    currentLine = next;
                    ShowLine();
                });
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    [System.Serializable]
    private class DialogueLineList
    {
        public DialogueLine[] lines;
    }
}
