using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Author UI")]
    public AdaptivePanel authorPanel;

    [Header("Left Character UI")]
    public AdaptivePanel leftCharacterPanel;
    public AdaptivePanel leftCharacterNamePanel;

    [Header("Other Character UI")]
    public AdaptivePanel otherPanel;
    public AdaptivePanel otherNamePanel;

    [Header("Choices UI")]
    public AdaptivePanel choicesPanel;
    public ChoiceButton[] choiceButtons;

    // ---------------------- HIDE ----------------------
    public void HideAll()
    {
        authorPanel?.gameObject.SetActive(false);

        leftCharacterPanel?.gameObject.SetActive(false);
        leftCharacterNamePanel?.gameObject.SetActive(false);

        otherPanel?.gameObject.SetActive(false);
        otherNamePanel?.gameObject.SetActive(false);

        choicesPanel?.gameObject.SetActive(false);

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    public void HideChoices()
    {
        choicesPanel?.gameObject.SetActive(false);

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    // ---------------------- AUTHOR ----------------------
    public void ShowAuthor(string text)
    {
        HideAll();

        authorPanel.gameObject.SetActive(true);

        if (authorPanel.targetText != null)
            authorPanel.targetText.text = text;

        authorPanel.RefreshSize();
    }

    // ---------------------- LEFT CHARACTER ----------------------
    public void ShowLeftCharacter(string name, string text)
    {
        HideAll();

        leftCharacterNamePanel.gameObject.SetActive(true);
        leftCharacterPanel.gameObject.SetActive(true);

        if (leftCharacterNamePanel.targetText != null)
            leftCharacterNamePanel.targetText.text = name;

        if (leftCharacterPanel.targetText != null)
            leftCharacterPanel.targetText.text = text;

        leftCharacterNamePanel.RefreshSize();
        leftCharacterPanel.RefreshSize();
    }

    // ---------------------- OTHER CHARACTER ----------------------
    public void ShowOther(string name, string text)
    {
        HideAll();

        otherNamePanel.gameObject.SetActive(true);
        otherPanel.gameObject.SetActive(true);

        if (otherNamePanel.targetText != null)
            otherNamePanel.targetText.text = name;

        if (otherPanel.targetText != null)
            otherPanel.targetText.text = text;

        otherNamePanel.RefreshSize();
        otherPanel.RefreshSize();
    }

    // ---------------------- CHOICES ----------------------
    public void ShowChoices(List<Choice> choices, Action<string> callback)
    {
        if (choices == null || choices.Count == 0)
        {
            HideChoices();
            return;
        }

        choicesPanel.gameObject.SetActive(true);

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

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