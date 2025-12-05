using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class AuthorResizePanel : MonoBehaviour
{
    [Header("Текст автора")]
    public TextMeshProUGUI targetText;

    [Header("Фиксированная ширина панели (X)")]
    public float fixedWidth = 900f;

    [Header("Высоты")]
    public float oneLineHeight = 150f;   // однострочный текст
    public float twoLinesHeight = 220f;  // две строки
    public float extraLineStep = 40f;    // на каждую строку сверху

    [Header("Ограничения")]
    public float minHeight = 130f;
    public float maxHeight = 600f;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void RefreshSize()
    {
        if (rect == null || targetText == null)
            return;

        targetText.ForceMeshUpdate();

        int lineCount = targetText.textInfo.lineCount;
        string text = targetText.text;

        bool longSingleLine = (lineCount == 1 && text.Length > 60);

        float targetHeight;

        if (lineCount <= 1 && !longSingleLine)
        {
            targetHeight = oneLineHeight;
        }
        else if (lineCount <= 2 || longSingleLine)
        {
            targetHeight = twoLinesHeight;
        }
        else
        {
            int extraLines = lineCount - 2;
            targetHeight = twoLinesHeight + extraLines * extraLineStep;
        }

        targetHeight = Mathf.Max(targetHeight, minHeight);
        if (maxHeight > 0f)
            targetHeight = Mathf.Min(targetHeight, maxHeight);

        rect.sizeDelta = new Vector2(fixedWidth, targetHeight);
    }
}
