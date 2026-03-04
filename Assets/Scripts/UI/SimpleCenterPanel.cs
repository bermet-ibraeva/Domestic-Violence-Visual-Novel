using UnityEngine;
using TMPro;

public class SimpleCenterPanel : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public float baseHeight = 150f; 
    public float pricePerLine = 30f;

    public void RefreshSize()
    {
        RectTransform rect = GetComponent<RectTransform>();
        RectTransform tRect = targetText.GetComponent<RectTransform>();

        // Фиксируем ширину текста по ширине панели
        tRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
        targetText.ForceMeshUpdate();

        int lines = Mathf.Max(1, targetText.textInfo.lineCount);
        float h = baseHeight + (lines - 1) * pricePerLine;

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

        // Центрируем текст (Pivot Y = 0.5 у текста и у панели)
        tRect.anchoredPosition = Vector2.zero;
    }
}