using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class AdaptivePanel : MonoBehaviour
{
    public TextMeshProUGUI targetText;

    [Header("Декоративный отступ (Borders)")]
    [Tooltip("Сколько пикселей сверху занимает 'шапка' спрайта (твои 200px)")]
    public float headerOffset = 200f;

    [Header("Настройки высоты")]
    public float baseHeight = 250f; 
    public float pricePerLine = 50f;
    public float minHeight = 250f;
    public float maxHeight = 600f;

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

        targetText.ForceMeshUpdate();
        int lineCount = Mathf.Max(1, targetText.textInfo.lineCount);

        // Расчет: высота контента + твоя "шапка" 200px
        float targetHeight = baseHeight + (lineCount - 1) * pricePerLine + headerOffset;
        
        targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight > 0 ? maxHeight : 10000f);

        // Рост вниз (Pivot Y должен быть 1)
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        // ПРИБИВАЕМ ТЕКСТ ПОД ШАПКУ
        if (textRect != null)
        {
            // Текст всегда начинается ПОСЛЕ декоративных 200 пикселей
            textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, -headerOffset);
        }
    }
}