using UnityEngine;
using System.Collections;

/*
LayoutController

This class manages the positioning of dialogue panels and choice buttons
for a visual novel or dialogue system.

Responsibilities:
- Holds references to dialogue panels:
    • AuthorPanel (narrator/author)
    • LeftPanel (left character)
    • RightPanel (right character)
- Holds references to choice buttons and their container
- Supports dynamic positioning of choice buttons relative to the active panel
- Provides RefreshButtons(RectTransform activePanelRect) to update button positions
    • Starts a coroutine to position buttons after the end of the frame
    • Aligns buttons to the horizontal center of the panel
    • Places buttons below the panel using buttonOffset
- Provides RefreshButtonPosition() to update button positions based on the currently active panel
- Designed to integrate with DialogueController to handle dialogue choices visually

Usage:
- Assign panels and choice buttons in the Inspector
- Call RefreshButtonPosition() after showing a dialogue panel
- Call RefreshButtons(panelRect) to manually reposition buttons for a specific panel
*/

public class LayoutController : MonoBehaviour
{
    [Header("Panels")]
    public SimpleCenterPanel AuthorPanel;
    public CharacterDialoguePanel LeftPanel;
    public CharacterDialoguePanel RightPanel;

    [Header("Choice Buttons")]
    public RectTransform ButtonsContainer;
    public ChoiceButton[] choiceButtons;
    public float buttonOffset = 20f;

    // Метод для позиционирования кнопок
    public void RefreshButtons(RectTransform activePanelRect)
    {
        if (activePanelRect == null) return;
        StartCoroutine(PositionRoutine(activePanelRect));
    }

    private IEnumerator PositionRoutine(RectTransform panel)
    {
        yield return new WaitForEndOfFrame();

        Vector3[] corners = new Vector3[4];
        panel.GetWorldCorners(corners);

        float bottomY = corners[0].y;
        float centerX = (corners[0].x + corners[3].x) / 2f;

        if (ButtonsContainer != null)
        {
            ButtonsContainer.position = new Vector3(centerX, bottomY - buttonOffset, panel.position.z);
        }
    }

    // Вспомогательный метод для обновления позиции без смены текста
    public void RefreshButtonPosition()
    {
        RectTransform active = null;
        if (AuthorPanel.gameObject.activeSelf) active = AuthorPanel.GetComponent<RectTransform>();
        else if (LeftPanel.gameObject.activeSelf) active = LeftPanel.GetComponent<RectTransform>();
        else if (RightPanel.gameObject.activeSelf) active = RightPanel.GetComponent<RectTransform>();

        if (active != null) RefreshButtons(active);
    }
}