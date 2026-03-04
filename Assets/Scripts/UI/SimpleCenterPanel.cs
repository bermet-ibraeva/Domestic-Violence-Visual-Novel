using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class SimpleCenterPanel : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    
    [Header("Настройки роста")]
    public float baseHeight = 150f; 
    public float pricePerLine = 30f;
    public float minHeight = 150f;
    public float maxHeight = 300f;

    private RectTransform rect;
    private RectTransform textRect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (targetText != null) textRect = targetText.GetComponent<RectTransform>();
    }

    public void RefreshSize()
    {
        if (rect == null || targetText == null) return;

        // Фиксируем ширину, чтобы расчет строк был верным
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
        targetText.ForceMeshUpdate();

        int lineCount = Mathf.Max(1, targetText.textInfo.lineCount);
        float targetHeight = baseHeight + (lineCount - 1) * pricePerLine;
        targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);

        // Применяем высоту
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        // Центруем текст по всем осям
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        targetText.alignment = TextAlignmentOptions.Center;
    }
}