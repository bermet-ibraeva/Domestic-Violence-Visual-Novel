using UnityEngine;
using TMPro;

public class CharacterDialoguePanel : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Panel Height")]
    public float minPanelHeight = 300f;
    public float maxPanelHeight = 520f;
    public float bottomPadding = 40f;
    public float textExtraPadding = 12f;

    public void RefreshSize()
    {
        CacheReferences();

        if (panelRect == null || dialogueText == null || textRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        dialogueText.ForceMeshUpdate();

        float textWidth = textRect.rect.width;
        if (textWidth <= 0f)
        {
            Debug.LogWarning($"[{gameObject.name}] text width <= 0", this);
            return;
        }

        float preferredHeight = dialogueText.GetPreferredValues(dialogueText.text, textWidth, 0f).y;
        preferredHeight += textExtraPadding;

        Vector2 offsetMax = textRect.offsetMax;
        offsetMax.y = -fixedTopOffset;
        textRect.offsetMax = offsetMax;

        Vector2 offsetMin = textRect.offsetMin;
        offsetMin.y = -(fixedTopOffset + preferredHeight);
        textRect.offsetMin = offsetMin;

        float panelHeight = fixedTopOffset + preferredHeight + bottomPadding;
        panelHeight = Mathf.Clamp(panelHeight, minPanelHeight, maxPanelHeight);

        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
    }
}