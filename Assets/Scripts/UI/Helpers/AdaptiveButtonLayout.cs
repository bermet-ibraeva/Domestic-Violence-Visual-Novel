using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AdaptiveButtonLayout : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform content;
    [SerializeField] private TextMeshProUGUI textUI;

    [Header("Size")]
    [SerializeField] private float minWidth = 800f;
    [SerializeField] private float minHeight = 120f;

    [SerializeField] private float maxWidth = 1000f;
    [SerializeField] private float maxHeight = 200f;

    [Header("Padding")]
    [SerializeField] private float paddingX = 60f;
    [SerializeField] private float paddingY = 40f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (rect == null || content == null)
            return;

        Canvas.ForceUpdateCanvases();

        if (textUI != null)
            textUI.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        Vector2 contentSize = content.rect.size;

        float width = Mathf.Clamp(contentSize.x + paddingX, minWidth, maxWidth);
        float height = Mathf.Clamp(contentSize.y + paddingY, minHeight, maxHeight);

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}