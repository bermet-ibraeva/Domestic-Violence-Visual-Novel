using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
UIController

This class manages the visual display of dialogue panels and choice buttons in a visual novel system.

Responsibilities:
- Integrates with LayoutController to control panels and buttons
- Provides methods to show/hide dialogue panels:
    • ShowAuthor(text)       -> displays narrator/author panel
    • ShowLeftCharacter(name, text)  -> displays left character panel
    • ShowRightCharacter(name, text) -> displays right character panel
    • HideAll()              -> hides all panels and choice buttons
- Manages choice buttons:
    • ShowChoices(choices, callback) -> displays choice buttons and binds their callbacks
    • HideChoices()                  -> hides all choice buttons
- Automatically refreshes button positions after showing any panel
- Designed to integrate with DialogueController for dynamic dialogue display

Usage:
- Call ShowAuthor, ShowLeftCharacter, or ShowRightCharacter to display dialogue
- Call ShowChoices to present branching options to the player
- Call HideAll to clear the UI
- Works with LayoutController for proper positioning of panels and buttons
*/

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