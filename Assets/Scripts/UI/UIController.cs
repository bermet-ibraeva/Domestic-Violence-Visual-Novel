using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Controllers")]
    public LayoutController layoutController;

    public void HideAll()
    {
        if (layoutController.AuthorPanel) layoutController.AuthorPanel.gameObject.SetActive(false);
        if (layoutController.LeftPanel) layoutController.LeftPanel.gameObject.SetActive(false);
        if (layoutController.RightPanel) layoutController.RightPanel.gameObject.SetActive(false);
        HideChoices();
    }

    public void HideChoices()
    {
        if (layoutController.ButtonsContainer != null)
            layoutController.ButtonsContainer.gameObject.SetActive(false);

        foreach (var btn in layoutController.choiceButtons)
            btn.gameObject.SetActive(false);
    }

    public void ShowAuthor(string text)
    {
        HideAll();
        layoutController.AuthorPanel.gameObject.SetActive(true);
        layoutController.AuthorPanel.targetText.text = text;
        layoutController.AuthorPanel.RefreshSize();
        layoutController.RefreshButtons(layoutController.AuthorPanel.GetComponent<RectTransform>());
    }

    public void ShowLeftCharacter(string name, string text)
    {
        HideAll();
        layoutController.LeftPanel.gameObject.SetActive(true);
        layoutController.LeftPanel.SetDialogue(name, text); 
        layoutController.RefreshButtons(layoutController.LeftPanel.GetComponent<RectTransform>());
    }

    public void ShowRightCharacter(string name, string text)
    {
        HideAll();
        layoutController.RightPanel.gameObject.SetActive(true);
        layoutController.RightPanel.SetDialogue(name, text);
        layoutController.RefreshButtons(layoutController.RightPanel.GetComponent<RectTransform>());
    }

    public void ShowChoices(List<Choice> choices, Action<string> callback)
    {
        if (choices == null || choices.Count == 0) return;

        layoutController.ButtonsContainer.gameObject.SetActive(true);
        foreach (var btn in layoutController.choiceButtons) btn.gameObject.SetActive(false);

        for (int i = 0; i < choices.Count; i++)
        {
            if (i >= layoutController.choiceButtons.Length) break;
            var btn = layoutController.choiceButtons[i];
            btn.gameObject.SetActive(true);
            string nextNode = choices[i].nextNode;
            btn.SetText(choices[i].text);
            btn.SetCallback(() => callback?.Invoke(nextNode));
        }
        layoutController.RefreshButtonPosition();
    }
}