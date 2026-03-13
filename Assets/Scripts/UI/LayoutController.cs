using UnityEngine;
using UnityEngine.UI;

public class LayoutController : MonoBehaviour
{
    [Header("Panels")]
    public SimpleCenterPanel AuthorPanel;
    public CharacterDialoguePanel LeftPanel;
    public CharacterDialoguePanel RightPanel;

    [Header("Choice Buttons")]
    public RectTransform ButtonsContainer;
    public ChoiceButton[] choiceButtons;
    public float buttonOffset = 15f;

    private RectTransform lastActivePanelRect;

    public void SetActivePanel(CharacterDialoguePanel panel)
    {
        if (panel == null) return;
        lastActivePanelRect = panel.GetComponent<RectTransform>();
    }

    public void SetActivePanel(SimpleCenterPanel panel)
    {
        if (panel == null) return;
        lastActivePanelRect = panel.GetComponent<RectTransform>();
    }

    public void SetActivePanel(RectTransform panelRect)
    {
        if (panelRect == null) return;
        lastActivePanelRect = panelRect;
    }

    public void RefreshButtonPosition()
    {
        if (lastActivePanelRect == null || ButtonsContainer == null)
            return;

        if (!lastActivePanelRect.gameObject.activeInHierarchy)
            return;

        RectTransform parentRect = ButtonsContainer.parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogWarning("[LayoutController] ButtonsContainer parent RectTransform not found.");
            return;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(lastActivePanelRect);

        Vector3[] corners = new Vector3[4];
        lastActivePanelRect.GetWorldCorners(corners);

        // Unity corners:
        // 0 = bottom-left
        // 1 = top-left
        // 2 = top-right
        // 3 = bottom-right

        Vector3 bottomLeftLocal = parentRect.InverseTransformPoint(corners[0]);
        Vector3 bottomRightLocal = parentRect.InverseTransformPoint(corners[3]);

        float targetX = (bottomLeftLocal.x + bottomRightLocal.x) * 0.5f;
        float targetY = bottomLeftLocal.y - buttonOffset;

        ButtonsContainer.anchoredPosition = new Vector2(targetX, targetY);
    }

    public void ClearActivePanel()
    {
        lastActivePanelRect = null;
    }
}