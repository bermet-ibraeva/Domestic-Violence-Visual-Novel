using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Handles the layout of choice buttons, ensuring they are positioned correctly relative to the target dialogue panel.

public class LayoutController : MonoBehaviour
{
    [Header("Choice Buttons")]
    [SerializeField] private RectTransform buttonsContainer;
    [SerializeField] private float buttonOffset = 35f;

    private RectTransform currentTargetRect;
    private Coroutine refreshRoutine;

    public RectTransform ButtonsContainer => buttonsContainer;

    public void SetTarget(RectTransform targetRect)
    {
        if (targetRect == null) return;
        currentTargetRect = targetRect;
    }

    public void ClearTarget()
    {
        currentTargetRect = null;
    }

    public void RefreshButtonPosition()
    {
        if (currentTargetRect == null || buttonsContainer == null)
            return;

        if (!currentTargetRect.gameObject.activeInHierarchy)
            return;

        RectTransform parentRect = buttonsContainer.parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogWarning("[LayoutController] ButtonsContainer parent RectTransform not found.");
            return;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(currentTargetRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsContainer);

        Vector3[] corners = new Vector3[4];
        currentTargetRect.GetWorldCorners(corners);

        Vector3 bottomLeftLocal = parentRect.InverseTransformPoint(corners[0]);
        Vector3 bottomRightLocal = parentRect.InverseTransformPoint(corners[3]);

        float targetX = (bottomLeftLocal.x + bottomRightLocal.x) * 0.5f;
        float targetY = bottomLeftLocal.y - buttonOffset;

        buttonsContainer.anchoredPosition = new Vector2(targetX, targetY);
    }

    public void RefreshButtonPositionDelayed()
    {
        if (refreshRoutine != null)
            StopCoroutine(refreshRoutine);

        refreshRoutine = StartCoroutine(RefreshButtonPositionNextFrame());
    }

    private IEnumerator RefreshButtonPositionNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        RefreshButtonPosition();
        refreshRoutine = null;
    }
}