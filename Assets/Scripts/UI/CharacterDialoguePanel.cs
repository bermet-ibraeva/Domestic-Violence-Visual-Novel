using UnityEngine;
using TMPro;

public class CharacterDialoguePanel : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Panel Settings")]
    public float baseHeight = 270f;     // 1 строка
    public float heightPerLine = 50f;   // шаг
    public int maxLines = 5;

    [Header("Text Area")]
    public float topOffset = 140f;
    public float bottomPadding = 40f;
    public float extraTextPadding = 12f;

    private RectTransform panelRect;
    private RectTransform textRect;

    void Awake()
    {
        CacheReferences();
    }

    void OnEnable()
    {
        CacheReferences();
    }

    void CacheReferences()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (dialogueText != null && textRect == null)
            textRect = dialogueText.GetComponent<RectTransform>();
    }

    public void SetDialogue(string characterName, string text)
    {
        CacheReferences();

        if (nameText != null)
            nameText.text = characterName;

        if (dialogueText != null)
            dialogueText.text = text;

        RefreshSize();
    }

    public void RefreshSize()
    {
        CacheReferences();

        if (panelRect == null || dialogueText == null || textRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        dialogueText.ForceMeshUpdate();

        float textWidth = textRect.rect.width;
        if (textWidth <= 0f)
            return;

        float preferredHeight = dialogueText.GetPreferredValues(dialogueText.text, textWidth, 0f).y;
        preferredHeight += extraTextPadding;

        // обновляем текстовую область
        Vector2 offsetMax = textRect.offsetMax;
        offsetMax.y = -topOffset;
        textRect.offsetMax = offsetMax;

        Vector2 offsetMin = textRect.offsetMin;
        offsetMin.y = -(topOffset + preferredHeight);
        textRect.offsetMin = offsetMin;

        Canvas.ForceUpdateCanvases();
        dialogueText.ForceMeshUpdate();

        // считаем строки
        int lineCount = Mathf.Max(1, dialogueText.textInfo.lineCount);
        lineCount = Mathf.Min(lineCount, maxLines);

        // считаем высоту панели
        float targetHeight = baseHeight + (lineCount - 1) * heightPerLine;

        float minimumNeeded = topOffset + preferredHeight + bottomPadding;
        if (targetHeight < minimumNeeded)
            targetHeight = minimumNeeded;

        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
    }
}