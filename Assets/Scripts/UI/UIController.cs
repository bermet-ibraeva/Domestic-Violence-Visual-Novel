using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    public AdaptivePanel authorPanel;
    public AdaptivePanel leftCharacterPanel;
    public AdaptivePanel rightCharacterPanel;

    [Header("Text Fields inside Panels")]
    public TMP_Text AuthorText;
    public TMP_Text LeftCharacterName;
    public TMP_Text LeftCharacterText;
    public TMP_Text RightCharacterName;
    public TMP_Text RightCharacterText;

    [Header("Choices")]
    public RectTransform choicesContainer;
    public ChoiceButton[] choiceButtons;

    // ---------------------- HIDE ----------------------
    public void HideAll()
    {
        authorPanel?.gameObject.SetActive(false);
        leftCharacterPanel?.gameObject.SetActive(false);
        rightCharacterPanel?.gameObject.SetActive(false);
        HideChoices();
    }

    public void HideChoices()
    {
        if (choicesContainer != null)
            choicesContainer.gameObject.SetActive(false);

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    // ---------------------- SHOW PANELS ----------------------
    public void ShowAuthor(string text)
    {
        HideAll();
        authorPanel.gameObject.SetActive(true);
        if (AuthorText != null)
            AuthorText.text = text;
        authorPanel.RefreshSize();
    }

    public void ShowLeftCharacter(string text)
    {
        leftCharacterPanel.gameObject.SetActive(true);
        if (leftCharacterPanel.targetText != null)
            leftCharacterPanel.targetText.text = text;
        leftCharacterPanel.RefreshSize();
    }

    public void ShowRightCharacter(string text)
    {
        rightCharacterPanel.gameObject.SetActive(true);
        if (rightCharacterPanel.targetText != null)
            rightCharacterPanel.targetText.text = text;
        rightCharacterPanel.RefreshSize();
    }

    // ---------------------- SHOW CHOICES ----------------------
    public void ShowChoices(List<Choice> choices, Action<string> callback)
    {
        if (choices == null || choices.Count == 0)
        {
            HideChoices();
            return;
        }

        choicesContainer.gameObject.SetActive(true);

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

        for (int i = 0; i < choices.Count; i++)
        {
            if (i >= choiceButtons.Length) break;

            ChoiceButton btn = choiceButtons[i];
            btn.gameObject.SetActive(true);

            string nextNode = choices[i].nextNode;
            btn.SetText(choices[i].text);
            btn.SetCallback(() => callback?.Invoke(nextNode));
        }
    }
}