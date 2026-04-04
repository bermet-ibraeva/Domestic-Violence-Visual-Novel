using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// manage dialogue panels, character roots, name panels, and choices

public class UIController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private LayoutController layoutController;

    [Header("Main Root")]
    [SerializeField] private GameObject dialogueAndChoiceRoot;

    [Header("Panels")]
    [SerializeField] private SimpleCenterPanel authorPanel;
    [SerializeField] private CharacterDialoguePanel leftPanel;
    [SerializeField] private CharacterDialoguePanel rightPanel;

    [Header("Dialogue Roots")]
    [SerializeField] private GameObject leftCharacterRootObject;
    [SerializeField] private GameObject rightCharacterRootObject;

    [Header("Name Panels")]
    [SerializeField] private GameObject leftNamePanelObject;
    [SerializeField] private GameObject rightNamePanelObject;

    [Header("Name Plates")]
    [SerializeField] private NamePlateAutoWidth leftNamePlate;
    [SerializeField] private NamePlateAutoWidth rightNamePlate;

    [Header("Choices")]
    [SerializeField] private ChoiceButton[] choiceButtons;

    public void HideAll()
    {
        HideDialoguePanels();
        HideCharacterRoots();
        HideChoices();

        if (dialogueAndChoiceRoot != null)
            dialogueAndChoiceRoot.SetActive(false);
    }

    public void HideDialoguePanels()
    {
        if (authorPanel != null)
            authorPanel.gameObject.SetActive(false);

        if (leftPanel != null)
            leftPanel.gameObject.SetActive(false);

        if (rightPanel != null)
            rightPanel.gameObject.SetActive(false);

        HideAllNamePanels();

        if (layoutController != null)
            layoutController.ClearTarget();
    }

    public void HideChoices()
    {
        if (layoutController != null && layoutController.ButtonsContainer != null)
            layoutController.ButtonsContainer.gameObject.SetActive(false);

        if (choiceButtons == null)
            return;

        foreach (var btn in choiceButtons)
        {
            if (btn != null)
                btn.gameObject.SetActive(false);
        }
    }

    public void HideCharacterRoots()
    {
        if (leftCharacterRootObject != null)
            leftCharacterRootObject.SetActive(false);

        if (rightCharacterRootObject != null)
            rightCharacterRootObject.SetActive(false);
    }

    public void HideAllNamePanels()
    {
        if (leftNamePanelObject != null)
            leftNamePanelObject.SetActive(false);

        if (rightNamePanelObject != null)
            rightNamePanelObject.SetActive(false);
    }

    public void ShowAuthor(string text)
    {
        EnsureDialogueRootActive();

        HideDialoguePanels();
        HideCharacterRoots();
        HideChoices();

        if (authorPanel == null)
            return;

        authorPanel.gameObject.SetActive(true);
        authorPanel.targetText.text = text;

        StartCoroutine(RefreshAuthorNextFrame());

        if (layoutController != null)
            layoutController.SetTarget(authorPanel.GetComponent<RectTransform>());
    }

    public void ShowLeftCharacter(string name, string text)
    {
        EnsureDialogueRootActive();

        HideDialoguePanels();
        HideChoices();

        if (leftPanel == null)
            return;

        if (leftCharacterRootObject != null)
            leftCharacterRootObject.SetActive(true);

        if (rightCharacterRootObject != null)
            rightCharacterRootObject.SetActive(false);

        leftPanel.gameObject.SetActive(true);
        leftPanel.SetDialogue(name, text);

        if (leftNamePlate != null)
            leftNamePlate.SetName(name);

        if (leftNamePanelObject != null)
            leftNamePanelObject.SetActive(true);

        if (rightNamePanelObject != null)
            rightNamePanelObject.SetActive(false);

        Canvas.ForceUpdateCanvases();

        if (layoutController != null)
            layoutController.SetTarget(leftPanel.GetComponent<RectTransform>());
    }

    public void ShowRightCharacter(string name, string text)
    {
        EnsureDialogueRootActive();

        HideDialoguePanels();
        HideChoices();

        if (rightPanel == null)
            return;

        if (leftCharacterRootObject != null)
            leftCharacterRootObject.SetActive(false);

        if (rightCharacterRootObject != null)
            rightCharacterRootObject.SetActive(true);

        rightPanel.gameObject.SetActive(true);
        rightPanel.SetDialogue(name, text);

        if (rightNamePlate != null)
            rightNamePlate.SetName(name);

        if (leftNamePanelObject != null)
            leftNamePanelObject.SetActive(false);

        if (rightNamePanelObject != null)
            rightNamePanelObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        if (layoutController != null)
            layoutController.SetTarget(rightPanel.GetComponent<RectTransform>());
    }

    public void ShowChoices(List<Choice> choices, Action<string> callback)
    {
        if (choices == null || choices.Count == 0 || choiceButtons == null)
            return;

        EnsureDialogueRootActive();

        if (layoutController != null && layoutController.ButtonsContainer != null)
            layoutController.ButtonsContainer.gameObject.SetActive(true);

        foreach (var btn in choiceButtons)
        {
            if (btn != null)
                btn.gameObject.SetActive(false);
        }

        for (int i = 0; i < choices.Count; i++)
        {
            if (i >= choiceButtons.Length)
                break;

            ChoiceButton btn = choiceButtons[i];
            if (btn == null)
                continue;

            string nextNode = choices[i].nextNode;
            string choiceText = choices[i].text;

            btn.gameObject.SetActive(true);
            btn.SetText(choiceText);
            btn.SetCallback(() => callback?.Invoke(nextNode));
        }

        Canvas.ForceUpdateCanvases();

        if (layoutController != null)
            layoutController.RefreshButtonPositionDelayed();
    }

    private void EnsureDialogueRootActive()
    {
        if (dialogueAndChoiceRoot != null && !dialogueAndChoiceRoot.activeSelf)
            dialogueAndChoiceRoot.SetActive(true);
    }

    private IEnumerator RefreshAuthorNextFrame()
    {
        yield return null;

        if (authorPanel != null)
            authorPanel.RefreshSize();

        Canvas.ForceUpdateCanvases();

        if (layoutController != null)
            layoutController.RefreshButtonPositionDelayed();
    }
}