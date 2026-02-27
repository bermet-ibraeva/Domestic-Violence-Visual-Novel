using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    public AdaptivePanel authorPanel;          // диалог автора
    public AdaptivePanel leftCharacterPanel;   // панель текста персонажа слева
    public AdaptivePanel rightCharacterPanel;  // панель текста персонажа справа

    [Header("Choices")]
    public RectTransform choicesPanel;         // контейнер кнопок
    public ChoiceButton[] choiceButtons;
    public float verticalOffset = 8f;          // px ниже активной панели

    // ---------------------- HIDE ----------------------
    public void HideAll()
    {
        authorPanel?.gameObject.SetActive(false);
        leftCharacterPanel?.gameObject.SetActive(false);
        rightCharacterPanel?.gameObject.SetActive(false);

        if (choicesPanel != null)
            choicesPanel.gameObject.SetActive(false);

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    public void HideChoices()
    {
        if (choicesPanel != null)
            choicesPanel.gameObject.SetActive(false);

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    // ---------------------- SHOW AUTHOR ----------------------
    public void ShowAuthor(string text)
    {
        HideAll();

        authorPanel.gameObject.SetActive(true);

        if (authorPanel.targetText != null)
            authorPanel.targetText.text = text;

        authorPanel.RefreshSize();

        PositionChoicesBelowActivePanel();
    }

    // ---------------------- SHOW LEFT CHARACTER ----------------------
    public void ShowLeftCharacter(string text)
    {
        HideAll();

        leftCharacterPanel.gameObject.SetActive(true);

        if (leftCharacterPanel.targetText != null)
            leftCharacterPanel.targetText.text = text;

        leftCharacterPanel.RefreshSize();

        PositionChoicesBelowActivePanel();
    }

    // ---------------------- SHOW RIGHT CHARACTER ----------------------
    public void ShowRightCharacter(string text)
    {
        HideAll();

        rightCharacterPanel.gameObject.SetActive(true);

        if (rightCharacterPanel.targetText != null)
            rightCharacterPanel.targetText.text = text;

        rightCharacterPanel.RefreshSize();

        PositionChoicesBelowActivePanel();
    }

    // ---------------------- SHOW CHOICES ----------------------
    public void ShowChoices(List<Choice> choices, Action<string> callback)
    {
        if (choices == null || choices.Count == 0)
        {
            HideChoices();
            return;
        }

        if (choicesPanel != null)
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

        PositionChoicesBelowActivePanel();
    }

    // ---------------------- POSITION CHOICES ----------------------
    private void PositionChoicesBelowActivePanel()
    {
        if (choicesPanel == null)
            return;

        AdaptivePanel activePanel = null;

        if (authorPanel != null && authorPanel.gameObject.activeSelf)
            activePanel = authorPanel;
        else if (leftCharacterPanel != null && leftCharacterPanel.gameObject.activeSelf)
            activePanel = leftCharacterPanel;
        else if (rightCharacterPanel != null && rightCharacterPanel.gameObject.activeSelf)
            activePanel = rightCharacterPanel;

        if (activePanel == null)
            return;

        RectTransform panelRect = activePanel.GetComponent<RectTransform>();
        if (panelRect == null)
            return;

        Vector3[] corners = new Vector3[4];
        panelRect.GetWorldCorners(corners);
        float panelBottomY = corners[0].y;

        Vector3 newPos = choicesPanel.position;
        newPos.y = panelBottomY - verticalOffset;

        // Горизонтально по центру активной панели
        newPos.x = (corners[0].x + corners[2].x) / 2f;

        choicesPanel.position = newPos;
    }
}