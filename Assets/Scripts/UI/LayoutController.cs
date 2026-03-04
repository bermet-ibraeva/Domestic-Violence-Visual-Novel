using UnityEngine;
using System.Collections;

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