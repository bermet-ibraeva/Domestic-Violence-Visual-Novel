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
    public float bottomPadding = 30f;

    private RectTransform panelRect;
    private RectTransform textRect;
    private float fixedTopOffset;

    void Awake()
    {
        CacheReferences();
    }

    void OnEnable()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (dialogueText != null && textRect == null)
            textRect = dialogueText.GetComponent<RectTransform>();

        if (textRect != null)
            fixedTopOffset = -textRect.offsetMax.y;
    }

    public void SetDialogue(string characterName, string text)
    {
        CacheReferences();

        if (nameText != null)
            nameText.text = characterName;
        else
            Debug.LogError($"[{gameObject.name}] nameText is NULL", this);

        if (dialogueText != null)
            dialogueText.text = text;
        else
            Debug.LogError($"[{gameObject.name}] dialogueText is NULL", this);

        RefreshSize();
    }

    public void RefreshSize()
    {
        CacheReferences();

        if (panelRect == null)
        {
            Debug.LogError($"[{gameObject.name}] panelRect is NULL", this);
            return;
        }

        if (dialogueText == null)
        {
            Debug.LogError($"[{gameObject.name}] dialogueText is NULL", this);
            return;
        }

        if (textRect == null)
        {
            Debug.LogError($"[{gameObject.name}] textRect is NULL", this);
            return;
        }

        Canvas.ForceUpdateCanvases();
        dialogueText.ForceMeshUpdate();

        float textWidth = textRect.rect.width;
        if (textWidth <= 0f)
        {
            Debug.LogWarning($"[{gameObject.name}] text width <= 0", this);
            return;
        }

        float preferredHeight = dialogueText.GetPreferredValues(dialogueText.text, textWidth, 0f).y;

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