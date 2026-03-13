using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Controllers")]
    public LayoutController layoutController;

    [Header("Roots")]
    public GameObject LeftCharacterRootObject;
    public GameObject RightCharacterRootObject;

    [Header("Name Panels")]
    public GameObject LeftNamePanelObject;
    public GameObject RightNamePanelObject;

    public void HideAll()
    {
        if (layoutController == null) return;

        if (layoutController.AuthorPanel != null)
            layoutController.AuthorPanel.gameObject.SetActive(false);

        if (LeftCharacterRootObject != null)
            LeftCharacterRootObject.SetActive(false);

        if (RightCharacterRootObject != null)
            RightCharacterRootObject.SetActive(false);

        HideAllNamePanels();
        HideChoices();
    }

    public void HideChoices()
    {
        if (layoutController == null) return;

        if (layoutController.ButtonsContainer != null)
            layoutController.ButtonsContainer.gameObject.SetActive(false);

        if (layoutController.choiceButtons == null) return;

        foreach (var btn in layoutController.choiceButtons)
        {
            if (btn != null)
                btn.gameObject.SetActive(false);
        }
    }

    public void HideAllNamePanels()
    {
        if (LeftNamePanelObject != null)
            LeftNamePanelObject.SetActive(false);

        if (RightNamePanelObject != null)
            RightNamePanelObject.SetActive(false);
    }

    public void ShowAuthor(string text)
    {
        HideAll();

        if (layoutController == null || layoutController.AuthorPanel == null)
            return;

        layoutController.AuthorPanel.gameObject.SetActive(true);
        layoutController.AuthorPanel.targetText.text = text;
        layoutController.AuthorPanel.RefreshSize();

        HideAllNamePanels();

        RectTransform authorRect = layoutController.AuthorPanel.GetComponent<RectTransform>();
        layoutController.RefreshButtons(authorRect);
    }

    public void ShowLeftCharacter(string name, string text)
    {
        HideAll();

        if (layoutController == null || layoutController.LeftPanel == null)
            return;

        if (LeftCharacterRootObject != null)
            LeftCharacterRootObject.SetActive(true);

        layoutController.LeftPanel.gameObject.SetActive(true);
        layoutController.LeftPanel.SetDialogue(name, text);

        if (LeftNamePanelObject != null)
            LeftNamePanelObject.SetActive(true);

        if (RightNamePanelObject != null)
            RightNamePanelObject.SetActive(false);

        if (layoutController.LeftCharacterRoot != null)
            layoutController.RefreshButtons(layoutController.LeftCharacterRoot);
    }

    public void ShowRightCharacter(string name, string text)
    {
        HideAll();

        if (layoutController == null || layoutController.RightPanel == null)
            return;

        if (RightCharacterRootObject != null)
            RightCharacterRootObject.SetActive(true);

        layoutController.RightPanel.gameObject.SetActive(true);
        layoutController.RightPanel.SetDialogue(name, text);

        if (LeftNamePanelObject != null)
            LeftNamePanelObject.SetActive(false);

        if (RightNamePanelObject != null)
            RightNamePanelObject.SetActive(true);

        if (layoutController.RightCharacterRoot != null)
            layoutController.RefreshButtons(layoutController.RightCharacterRoot);
    }

    public void ShowChoices(List<Choice> choices, Action<string> callback)
    {
        if (layoutController == null || choices == null || choices.Count == 0)
            return;

        if (layoutController.ButtonsContainer != null)
            layoutController.ButtonsContainer.gameObject.SetActive(true);

        if (layoutController.choiceButtons == null) return;

        foreach (var btn in layoutController.choiceButtons)
        {
            if (btn != null)
                btn.gameObject.SetActive(false);
        }

        for (int i = 0; i < choices.Count; i++)
        {
            if (i >= layoutController.choiceButtons.Length)
                break;

            var btn = layoutController.choiceButtons[i];
            if (btn == null) continue;

            btn.gameObject.SetActive(true);

            string nextNode = choices[i].nextNode;
            btn.SetText(choices[i].text);
            btn.SetCallback(() => callback?.Invoke(nextNode));
        }

        layoutController.RefreshButtonPosition();
    }
}