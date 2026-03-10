using UnityEngine;
using TMPro;

public class CharacterDialoguePanel : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;

    [Header("Panel Settings")]
    public float topPadding = 150f;
    public float bottomPadding = 40f;
    public float leftRightPadding = 50f;
    public float minHeight = 280f;

    private RectTransform panelRect;
    private RectTransform textRect;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();

        if (dialogueText != null)
            textRect = dialogueText.GetComponent<RectTransform>();
    }

    // Оставили для совместимости со старым UIController
    public void SetDialogue(string name, string text)
    {
        SetText(text);
    }

    public void SetText(string text)
    {
        if (dialogueText != null)
            dialogueText.text = text;

        RefreshSize();
    }

    public void RefreshSize()
    {
        if (panelRect == null || dialogueText == null || textRect == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing references.");
            return;
        }

        Canvas.ForceUpdateCanvases();

        float textWidth = panelRect.rect.width - (leftRightPadding * 2f);
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

        dialogueText.ForceMeshUpdate();

        float textHeight = dialogueText.preferredHeight;
        float targetHeight = topPadding + textHeight + bottomPadding;

        if (targetHeight < minHeight)
            targetHeight = minHeight;

        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        textRect.anchoredPosition = new Vector2(0f, -topPadding);
    }
}