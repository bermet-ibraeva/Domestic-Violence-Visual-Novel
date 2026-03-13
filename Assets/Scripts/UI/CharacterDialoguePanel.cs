using UnityEngine;
using TMPro;

public class CharacterDialoguePanel : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Panel Height")]
    public float minPanelHeight = 280f;
    public float maxPanelHeight = 400f;
    public float bottomPadding = 20f;

    private RectTransform panelRect;
    private RectTransform textRect;

    private float fixedTopOffset;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();

        if (dialogueText != null)
            textRect = dialogueText.GetComponent<RectTransform>();

        if (textRect != null)
            fixedTopOffset = -textRect.offsetMax.y; // если Top = 140, тут сохранится 140
    }

    public void SetDialogue(string characterName, string text)
    {
        if (nameText != null)
            nameText.text = characterName;

        if (dialogueText != null)
            dialogueText.text = text;

        RefreshSize();
    }

    public void RefreshSize()
    {
        if (panelRect == null || textRect == null || dialogueText == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing references.");
            return;
        }

        Canvas.ForceUpdateCanvases();
        dialogueText.ForceMeshUpdate();

        float textWidth = textRect.rect.width;
        if (textWidth <= 0f)
        {
            Debug.LogWarning($"[{gameObject.name}] text width <= 0");
            return;
        }

        float preferredHeight = dialogueText.GetPreferredValues(dialogueText.text, textWidth, 0f).y;

        // ЖЕСТКО возвращаем Top обратно, чтобы он всегда был 140
        Vector2 offsetMax = textRect.offsetMax;
        offsetMax.y = -fixedTopOffset;
        textRect.offsetMax = offsetMax;

        // Меняем только низ текста, чтобы он рос вниз
        Vector2 offsetMin = textRect.offsetMin;
        offsetMin.y = -(fixedTopOffset + preferredHeight);
        textRect.offsetMin = offsetMin;

        float panelHeight = fixedTopOffset + preferredHeight + bottomPadding;
        panelHeight = Mathf.Clamp(panelHeight, minPanelHeight, maxPanelHeight);

        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
    }
}