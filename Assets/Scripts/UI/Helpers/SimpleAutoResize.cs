using UnityEngine;
using TMPro;

public class SimpleAutoResize : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textUI;

    [Header("Panel Height")]
    public float minHeight = 200f;
    public float maxHeight = 500f;
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

        if (textUI != null && textRect == null)
            textRect = textUI.GetComponent<RectTransform>();

        if (textRect != null)
            fixedTopOffset = -textRect.offsetMax.y;
    }

    public void SetText(string text)
    {
        CacheReferences();

        if (textUI == null)
        {
            Debug.LogError($"[{gameObject.name}] textUI is NULL", this);
            return;
        }

        textUI.text = text;
        RefreshSize();
    }

    public void RefreshSize()
    {
        CacheReferences();

        if (panelRect == null || textUI == null || textRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        textUI.ForceMeshUpdate();

        float textWidth = textRect.rect.width;
        if (textWidth <= 0f)
        {
            Debug.LogWarning($"[{gameObject.name}] text width <= 0", this);
            return;
        }

        float preferredHeight = textUI.GetPreferredValues(textUI.text, textWidth, 0f).y;

        Vector2 offsetMax = textRect.offsetMax;
        offsetMax.y = -fixedTopOffset;
        textRect.offsetMax = offsetMax;

        Vector2 offsetMin = textRect.offsetMin;
        offsetMin.y = -(fixedTopOffset + preferredHeight);
        textRect.offsetMin = offsetMin;

        float panelHeight = fixedTopOffset + preferredHeight + bottomPadding;
        panelHeight = Mathf.Clamp(panelHeight, minHeight, maxHeight);

        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
    }
}