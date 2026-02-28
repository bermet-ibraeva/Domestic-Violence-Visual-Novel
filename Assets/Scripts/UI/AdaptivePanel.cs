using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class AdaptivePanel : MonoBehaviour
{
    [Header("Текстовый компонент")]
    public TextMeshProUGUI targetText;

    [Header("Padding & Constraints")]
    [Tooltip("Дополнительное пространство сверху и снизу")]
    public float verticalPadding = 30f;

    [Tooltip("Минимальная высота панели")]
    public float minHeight = 100f;

    [Tooltip("Максимальная высота панели, 0 = без ограничения")]
    public float maxHeight = 0f;

    [Header("Ширина панели")]
    [Tooltip("Если >0, используется фиксированная ширина для расчёта переноса текста")]
    public float fixedWidth = 0f;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Автоматически подгоняет высоту панели под текст
    /// </summary>
    public void RefreshSize()
    {
        if (rect == null || targetText == null) return;

        targetText.ForceMeshUpdate();

        // Ширина для расчёта переноса
        float width = fixedWidth > 0 ? fixedWidth : rect.rect.width;

        // Получаем предпочтительные размеры под заданную ширину
        Vector2 preferred = targetText.GetPreferredValues(targetText.text, width, 0f);

        float newHeight = preferred.y + verticalPadding;

        // Ограничения по min/max
        if (minHeight > 0f) newHeight = Mathf.Max(newHeight, minHeight);
        if (maxHeight > 0f) newHeight = Mathf.Min(newHeight, maxHeight);

        // Применяем к RectTransform
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);

        // Принудительно обновим Canvas для корректного отображения
        Canvas.ForceUpdateCanvases();
    }
}