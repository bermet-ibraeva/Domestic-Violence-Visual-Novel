using UnityEngine;

public class ChoiceButtonPositioner : MonoBehaviour
{
    public RectTransform AuthorPanel;
    public RectTransform LeftCharacterPanel;
    public RectTransform RightCharacterPanel;
    public RectTransform ButtonsContainer; // parent для кнопок
    public float verticalOffset = 8f; // px ниже панели

    public void UpdateButtonPosition()
    {
        RectTransform activePanel = null;

        // Определяем активную панель
        if (AuthorPanel.gameObject.activeSelf)
            activePanel = AuthorPanel;
        else if (LeftCharacterPanel.gameObject.activeSelf)
            activePanel = LeftCharacterPanel;
        else if (RightCharacterPanel.gameObject.activeSelf)
            activePanel = RightCharacterPanel;

        if (activePanel == null)
            return;

        // Берём нижнюю точку панели
        Vector3[] corners = new Vector3[4];
        activePanel.GetWorldCorners(corners);
        float panelBottomY = corners[0].y;

        // Устанавливаем кнопки чуть ниже панели
        Vector3 buttonsPos = ButtonsContainer.position;
        buttonsPos.y = panelBottomY - verticalOffset;

        // Горизонтально по центру панели
        buttonsPos.x = (corners[0].x + corners[2].x) / 2f;

        ButtonsContainer.position = buttonsPos;
    }
}