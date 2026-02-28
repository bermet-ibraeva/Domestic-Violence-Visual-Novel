using UnityEngine;
using System.Collections;

public class LayoutController : MonoBehaviour
{
    [Header("Dialogue Panels")]
    public AdaptivePanel AuthorPanel;
    public AdaptivePanel LeftCharacterPanel;
    public AdaptivePanel RightCharacterPanel;

    [Header("Choice Buttons")]
    public RectTransform ButtonsContainer;
    public float buttonOffset = 8f;

    public void ApplyLayout(string characterType)
    {
        // Скрыть все панели
        AuthorPanel.gameObject.SetActive(false);
        LeftCharacterPanel.gameObject.SetActive(false);
        RightCharacterPanel.gameObject.SetActive(false);

        AdaptivePanel activePanel = null;

        // Включить нужную панель
        switch (characterType)
        {
            case "Narrator":
                AuthorPanel.gameObject.SetActive(true);
                activePanel = AuthorPanel;
                break;

            case "LeftCharacter":
                LeftCharacterPanel.gameObject.SetActive(true);
                activePanel = LeftCharacterPanel;
                break;

            case "RightCharacter":
                RightCharacterPanel.gameObject.SetActive(true);
                activePanel = RightCharacterPanel;
                break;
        }

        if (activePanel != null)
        {
            // Принудительный ресайз панели
            activePanel.RefreshSize();

            // Позиционируем кнопки через пару кадров, чтобы layout успел пересчитать
            StartCoroutine(PositionButtonsNextFrame(activePanel));
        }
    }

    private IEnumerator PositionButtonsNextFrame(AdaptivePanel activePanel)
    {
        // Ждём 1 кадр
        yield return null;

        // Принудительно обновляем Canvas
        Canvas.ForceUpdateCanvases();

        if (activePanel == null || ButtonsContainer == null) yield break;

        RectTransform panelRect = activePanel.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        panelRect.GetWorldCorners(corners);

        float bottomY = corners[0].y;
        float centerX = (corners[0].x + corners[2].x) / 2f;

        Vector3 pos = ButtonsContainer.position;
        pos.x = centerX;
        pos.y = bottomY - buttonOffset;

        ButtonsContainer.position = pos;
    }

    // Для ручного обновления, если панель меняется динамически после ShowNode
    public void RefreshButtonPosition()
    {
        AdaptivePanel activePanel = null;
        if (AuthorPanel.gameObject.activeSelf) activePanel = AuthorPanel;
        else if (LeftCharacterPanel.gameObject.activeSelf) activePanel = LeftCharacterPanel;
        else if (RightCharacterPanel.gameObject.activeSelf) activePanel = RightCharacterPanel;

        if (activePanel != null)
            StartCoroutine(PositionButtonsNextFrame(activePanel));
    }
}