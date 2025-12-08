using UnityEngine;
using TMPro;

public class LayoutController : MonoBehaviour
{
    [Header("Панели диалога (RectTransform)")]
    public RectTransform AuthorPanel;
    public RectTransform AinazPanel;
    public RectTransform OtherPanel;

    [Header("Тексты")]
    public TextMeshProUGUI AuthorText;
    public TextMeshProUGUI AinazText;
    public TextMeshProUGUI OtherText;

    [Header("Ресайзеры")]
    public AuthorResizePanel AuthorResize;
    public AutoResizePanel AinazResize;
    public AutoResizePanel OtherResize;

    [Header("Панель выборов")]
    public RectTransform ChoicePanel;
    public AutoHeightChoicesGroup ChoicesGroup;

    [Header("Отступ между диалогом и выбором (в пикселях)")]
    public float gap = 8f;   // можно уже не использовать, если пойдём по фикс-Y

    [Header("Y-позиция ChoicePanel")]
    public float choiceYForTwoLines = -280f;
    public float choiceYForThreePlus = -300f;


    private TextMeshProUGUI activeText;
    private MonoBehaviour activeResizer;
    private RectTransform activePanel;


    // ============ вызывается из DialogueController ============
    public void ApplyLayout(string character)
    {
        // 1. Определяем, какая панель сейчас активна
        switch (character)
        {
            case "Автор":
                activeText = AuthorText;
                activeResizer = AuthorResize;
                activePanel = AuthorPanel;
                break;

            case "Айназ":
                activeText = AinazText;
                activeResizer = AinazResize;
                activePanel = AinazPanel;
                break;

            default:
                activeText = OtherText;
                activeResizer = OtherResize;
                activePanel = OtherPanel;
                break;
        }

        // 2. Обновляем высоту панели диалога
        RefreshDialogueSize();

        // 3. Обновляем высоту кнопок (если текст в них поменялся)
        if (ChoicesGroup != null)
            ChoicesGroup.Refresh();

        // 4. Сдвигаем ChoicePanel под диалог с нужным отступом
        RefreshChoicePosition();
    }


    private void RefreshDialogueSize()
    {
        if (activeResizer is AuthorResizePanel ar)
            ar.RefreshSize();
        else if (activeResizer is AutoResizePanel rp)
            rp.RefreshSize();
    }


    private void RefreshChoicePosition()
    {
        if (ChoicePanel == null || activeText == null)
            return;

        // пересчитать строки в текущем диалоге
        activeText.ForceMeshUpdate();
        int lines = activeText.textInfo.lineCount;
        string text = activeText.text;

        // если одна длинная строка — считаем как две
        bool longSingleLine = (lines == 1 && text.Length > 40);
        if (longSingleLine)
            lines = 2;

        // выбираем целевой Y
        float targetY = (lines <= 2) ? choiceYForTwoLines : choiceYForThreePlus;

        Vector2 pos = ChoicePanel.anchoredPosition;
        pos.y = targetY;
        ChoicePanel.anchoredPosition = pos;
    }
}