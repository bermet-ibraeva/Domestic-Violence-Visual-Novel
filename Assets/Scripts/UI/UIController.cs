using System;
using UnityEngine;
using TMPro;


public class UIController : MonoBehaviour
{
    [Header("Author UI")]
    public GameObject authorPanel;
    public TextMeshProUGUI authorText;

    [Header("Ainaz UI")]
    public GameObject ainazPanel;
    public GameObject ainazNamePanel;        // НОВАЯ ПАНЕЛЬ ИМЕНИ
    public TextMeshProUGUI ainazName;
    public TextMeshProUGUI ainazText;

    [Header("Other character UI")]
    public GameObject otherPanel;
    public GameObject otherNamePanel;        // НОВАЯ ПАНЕЛЬ ИМЕНИ
    public TextMeshProUGUI otherName;
    public TextMeshProUGUI otherText;

    [Header("Choices UI")]
    public GameObject choicesPanel;
    public ChoiceButton[] choiceButtons;

    // ---------------------- HIDE ----------------------

    public void HideAll()
    {
        authorPanel?.SetActive(false);

        ainazPanel?.SetActive(false);
        ainazNamePanel?.SetActive(false);

        otherPanel?.SetActive(false);
        otherNamePanel?.SetActive(false);

        choicesPanel?.SetActive(false);

        // Скрыть все кнопки
        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    public void HideChoices()
    {
        choicesPanel?.SetActive(false);

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    // ---------------------- AUTHOR ----------------------

    public void ShowAuthor(string text)
    {
        HideAll();

        authorPanel.SetActive(true);
        authorText.text = text;

        AutoResizePanel auto = authorPanel.GetComponent<AutoResizePanel>();
        if (auto) auto.RefreshSize();
    }

    // ---------------------- AINAZ ----------------------

    public void ShowAinaz(string name, string text)
    {
        HideAll();

        ainazNamePanel.SetActive(true);
        ainazPanel.SetActive(true);

        ainazName.text = name;
        ainazText.text = text;

        // resize name panel
        AutoResizePanel nameAuto = ainazNamePanel.GetComponent<AutoResizePanel>();
        if (nameAuto) nameAuto.RefreshSize();

        // resize dialogue panel
        AutoResizePanel textAuto = ainazPanel.GetComponent<AutoResizePanel>();
        if (textAuto) textAuto.RefreshSize();
    }

    // ---------------------- OTHER ----------------------

    public void ShowOther(string name, string text)
    {
        HideAll();

        otherNamePanel.SetActive(true);
        otherPanel.SetActive(true);

        otherName.text = name;
        otherText.text = text;

        AutoResizePanel nameAuto = otherNamePanel.GetComponent<AutoResizePanel>();
        if (nameAuto) nameAuto.RefreshSize();

        AutoResizePanel textAuto = otherPanel.GetComponent<AutoResizePanel>();
        if (textAuto) textAuto.RefreshSize();
    }


    // ---------------------- CHOICES ----------------------

    public void ShowChoices(System.Collections.Generic.List<Choice> choices, System.Action<string> callback)
    {
        if (choices == null || choices.Count == 0)
        {
            HideChoices();
            return;
        }

        choicesPanel.SetActive(true);

        // Скрываем все кнопки
        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

        // Показываем только нужное количество кнопок
        for (int i = 0; i < choices.Count; i++)
        {
            if (i >= choiceButtons.Length)
            {
                Debug.LogWarning("UIController: choice count > number of UI buttons!");
                break;
            }

            ChoiceButton btn = choiceButtons[i];
            btn.gameObject.SetActive(true);

            string nextNode = choices[i].nextNode;
            btn.SetText(choices[i].text);
            btn.SetCallback(() => callback?.Invoke(nextNode));
        }
    }
}
