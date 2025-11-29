using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class AutoResizePanel : MonoBehaviour
{
    [Header("Какой текст учитывать")]
    public TextMeshProUGUI targetText;

    [Header("Ширина панели (по X)")]
    public float fixedWidth = 900f;      // длина по горизонтали

    [Header("Высота панели (по Y)")]
    public float minHeight = 160f;       // минимальная высота
    public float maxHeight = 600f;       // ограничение сверху, если нужно

    [Header("Вертикальные отступы")]
    public float paddingY = 30f;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void RefreshSize()
    {
        if (rect == null || targetText == null) return;

        // Обновляем layout, чтобы preferredHeight была актуальна
        LayoutRebuilder.ForceRebuildLayoutImmediate(targetText.rectTransform);

        // Берём высоту текста + паддинги
        float height = targetText.preferredHeight + paddingY;

        // Минимальная высота
        if (height < minHeight)
            height = minHeight;

        // Максимальная, если нужна
        if (maxHeight > 0f)
            height = Mathf.Min(height, maxHeight);

        // Ширина всегда фиксированная, высота динамическая
        rect.sizeDelta = new Vector2(fixedWidth, height);
    }
}
