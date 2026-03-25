using UnityEngine;
using TMPro;

public class SimpleCenterPanel : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public float baseHeight = 140f;
    public float pricePerLine = 25f;
    public float singleLineHeight = 30f;

    public void RefreshSize()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null || targetText == null) return;

        RectTransform tRect = targetText.GetComponent<RectTransform>();
        if (tRect == null) return;

        tRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);

        Canvas.ForceUpdateCanvases();
        targetText.ForceMeshUpdate();

        float preferredHeight = targetText.preferredHeight;
        float h = Mathf.Max(baseHeight, preferredHeight + 40f);

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        tRect.anchoredPosition = Vector2.zero;
    }
}