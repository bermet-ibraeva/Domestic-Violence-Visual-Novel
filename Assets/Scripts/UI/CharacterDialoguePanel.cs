using UnityEngine;
using TMPro;

public class CharacterDialoguePanel : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Panel")]
    public float minHeight = 280f;
    public float maxHeight = 400f;

    [Header("Offsets")]
    public float textTopOffset = 140f;
    public float bottomPadding = 20f;

    private RectTransform panelRect;
    private RectTransform textRect;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        textRect = dialogueText.GetComponent<RectTransform>();
    }

    public void SetDialogue(string characterName, string text)
    {
        if (nameText != null)
            nameText.text = characterName;

        dialogueText.text = text;

        RefreshSize();
    }

    public void RefreshSize()
    {
        Canvas.ForceUpdateCanvases();
        dialogueText.ForceMeshUpdate();

        float width = textRect.rect.width;

        float textHeight = dialogueText
            .GetPreferredValues(dialogueText.text, width, 0)
            .y;

        textRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            textHeight
        );

        float panelHeight = textTopOffset + textHeight + bottomPadding;

        panelHeight = Mathf.Clamp(panelHeight, minHeight, maxHeight);

        panelRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            panelHeight
        );
    }
}