using UnityEngine;
using UnityEngine.UI;

public class ButtonContainerFollower : MonoBehaviour
{
    [Header("References")]
    public RectTransform buttonContainer;

    [Header("Dialogue panels")]
    public RectTransform leftCharacterPanel;
    public RectTransform rightCharacterPanel;
    public RectTransform authorPanel;

    [Header("Settings")]
    public float offsetY = 8f;

    private RectTransform currentTarget;
    private RectTransform buttonParent;

    private void Awake()
    {
        if (buttonContainer != null)
            buttonParent = buttonContainer.parent as RectTransform;
    }

    public void FollowLeft()
    {
        FollowTarget(leftCharacterPanel);
    }

    public void FollowRight()
    {
        FollowTarget(rightCharacterPanel);
    }

    public void FollowAuthor()
    {
        FollowTarget(authorPanel);
    }

    public void FollowTarget(RectTransform target)
    {
        currentTarget = target;
        RefreshPosition();
    }

    public void RefreshPosition()
    {
        if (buttonContainer == null || buttonParent == null || currentTarget == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(currentTarget);

        Vector3[] corners = new Vector3[4];
        currentTarget.GetWorldCorners(corners);

        // Unity:
        // 0 = bottom-left
        // 1 = top-left
        // 2 = top-right
        // 3 = bottom-right

        Vector3 bottomLeft = buttonParent.InverseTransformPoint(corners[0]);
        Vector3 bottomRight = buttonParent.InverseTransformPoint(corners[3]);

        float targetX = (bottomLeft.x + bottomRight.x) * 0.5f;
        float targetY = bottomLeft.y - offsetY;

        buttonContainer.anchoredPosition = new Vector2(targetX, targetY);
    }
}