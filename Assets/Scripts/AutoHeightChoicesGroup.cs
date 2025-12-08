using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class AutoHeightChoicesGroup : MonoBehaviour
{
    [Header("Кнопки с вариантами выбора")]
    public Button[] choiceButtons;

    [Header("Фиксированный размер по X")]
    public float width = 820f;

    [Header("Высоты по количеству строк")]
    public float oneLineHeight = 120f;   // 1 строка
    public float twoLinesHeight = 160f;  // 2 строки
    public float extraLineStep = 40f;    // за каждую строку сверх двух

    [Header("Ограничения по высоте")]
    public float minHeight = 120f;
    public float maxHeight = 260f;

    [Header("Расстояние между кнопками")]
    public float spacing = 8f;

    private VerticalLayoutGroup layoutGroup;

    void Awake()
    {
        layoutGroup = GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = spacing;
        layoutGroup.childControlHeight = false;     // высоту задаём сами
        layoutGroup.childForceExpandHeight = false;
    }

    /// <summary>
    /// Вызывать после того, как во все кнопки записан текст.
    /// </summary>
    public void RefreshLayout()
    {
        if (choiceButtons == null || choiceButtons.Length == 0)
            return;

        int maxLines = 1;

        // 1) Считаем максимальноe кол-во строк среди всех кнопок
        foreach (var btn in choiceButtons)
        {
            if (btn == null) continue;

            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null) continue;

            tmp.ForceMeshUpdate();

            int lineCount = tmp.textInfo.lineCount;
            string text = tmp.text;

            // если одна длинная строка — считаем как две
            bool longSingleLine = (lineCount == 1 && text.Length > 40);
            if (longSingleLine)
                lineCount = 2;

            if (lineCount > maxLines)
                maxLines = lineCount;
        }

        // 2) Переводим maxLines → высоту
        float targetHeight;

        if (maxLines <= 1)
        {
            targetHeight = oneLineHeight;
        }
        else if (maxLines == 2)
        {
            targetHeight = twoLinesHeight;
        }
        else
        {
            int extraLines = maxLines - 2;
            targetHeight = twoLinesHeight + extraLines * extraLineStep;
        }

        targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);

        // 3) Применяем одинаковую высоту ко всем кнопкам
        foreach (var btn in choiceButtons)
        {
            if (btn == null) continue;

            RectTransform rect = btn.GetComponent<RectTransform>();
            if (rect == null) continue;

            rect.sizeDelta = new Vector2(width, targetHeight);
        }

        // 4) Перестраиваем лейаут
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            GetComponent<RectTransform>()
        );
    }
}
