using UnityEngine;

public class LayoutController : MonoBehaviour
{
    [Header("Panels")]
    public SimpleCenterPanel AuthorPanel;
    public CharacterDialoguePanel LeftPanel;
    public CharacterDialoguePanel RightPanel;

    [Header("Character Roots")]
    public RectTransform LeftCharacterRoot;
    public RectTransform RightCharacterRoot;

    [Header("Choice Buttons")]
    public RectTransform ButtonsContainer;
    public ChoiceButton[] choiceButtons;
    public float buttonOffset = 30f;

    private RectTransform lastActiveRoot;

    public void RefreshButtons(RectTransform activeRoot)
    {
        if (activeRoot == null || ButtonsContainer == null)
            return;

        lastActiveRoot = activeRoot;

        Canvas.ForceUpdateCanvases();
        PositionButtonsBelowRoot(activeRoot);
    }

    public void RefreshButtonPosition()
    {
        if (lastActiveRoot == null || ButtonsContainer == null)
            return;

        Canvas.ForceUpdateCanvases();
        PositionButtonsBelowRoot(lastActiveRoot);
    }

    private void PositionButtonsBelowRoot(RectTransform root)
    {
        if (root == null || ButtonsContainer == null)
            return;

        Canvas canvas = ButtonsContainer.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[LayoutController] Canvas not found.");
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogWarning("[LayoutController] Canvas RectTransform not found.");
            return;
        }

        Vector3[] corners = new Vector3[4];
        root.GetWorldCorners(corners);

        // 0 = bottom-left, 3 = top-left
        Vector3 worldBottomCenter = (corners[0] + corners[3]) * 0.5f;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, worldBottomCenter),
            null,
            out localPoint
        );

        float targetX = localPoint.x;
        float targetY = localPoint.y - buttonOffset;

        ButtonsContainer.anchoredPosition = new Vector2(targetX, targetY);
    }
}