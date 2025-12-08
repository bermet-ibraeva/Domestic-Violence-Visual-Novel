using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class AutoResizePanel : MonoBehaviour
{
    [Header("Текст, от которого считаем высоту")]
    public TextMeshProUGUI targetText;

    [Header("Фиксированная ширина панели (X)")]
    public float fixedWidth = 900f;

    [Header("Высоты для центра (автор)")]
    public float oneLineHeight = 260f;   // ~1 строка
    public float twoLinesHeight = 260f;  // ~2 строки
    public float extraLineStep = 40f;    // на каждую строку сверху (3+, если вдруг)

    [Header("Ограничения")]
    public float minHeight = 260f;
    public float maxHeight = 400f;

    [Header("Вертикальные отступы")]
    public float paddingY = 0f;          // если хочешь ещё добавить сверху/снизу

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void RefreshSize()
    {
        if (rect == null || targetText == null)
            return;

        // Обновляем меш, чтобы была актуальная информация о строках
        targetText.ForceMeshUpdate();

        int lineCount = targetText.textInfo.lineCount;
        string text = targetText.text;

        // Спец-правило: если одна строка, но текст длинный (>60 символов) — считаем как 2 строки
        bool longSingleLine = (lineCount == 1 && text.Length > 60);

        float targetHeight;

        if (lineCount <= 1 && !longSingleLine)
        {
            // Одна короткая строка
            targetHeight = oneLineHeight;
        }
        else if (lineCount <= 2 || longSingleLine)
        {
            // Две строки ИЛИ длинная одна — берём высоту как под 2 строки
            targetHeight = twoLinesHeight;
        }
        else
        {
            // 3+ строки — наращиваем дальше
            int extraLines = lineCount - 2;
            targetHeight = twoLinesHeight + extraLines * extraLineStep;
        }

        // Паддинги, если хочешь
        targetHeight += paddingY;

        // Клапаем в лимиты
        targetHeight = Mathf.Max(targetHeight, minHeight);
        if (maxHeight > 0f)
            targetHeight = Mathf.Min(targetHeight, maxHeight);

        // Ширина фиксированная
        rect.sizeDelta = new Vector2(fixedWidth, targetHeight);
    }
}
