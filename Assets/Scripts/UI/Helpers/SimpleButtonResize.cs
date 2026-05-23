using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleButtonResize : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public float minHeight = 160f;
    public float verticalPadding = 70f;

    private RectTransform rect;
    private LayoutElement layoutElement;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();
    }

    public void RefreshSize()
    {
        if (rect == null || targetText == null) return;

        Canvas.ForceUpdateCanvases();
        targetText.ForceMeshUpdate();

        float textHeight = targetText.preferredHeight;
        float finalHeight = Mathf.Max(minHeight, textHeight + verticalPadding);

        layoutElement.minHeight = finalHeight;
        layoutElement.preferredHeight = finalHeight;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rect.parent as RectTransform);
    }
}