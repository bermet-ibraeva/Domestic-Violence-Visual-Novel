using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class TextMaxWidthClamp : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public float maxWidth = 480f;

    private LayoutElement layoutElement;

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();

        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();
    }

    public void RefreshWidth()
    {
        if (layoutElement == null)
            layoutElement = GetComponent<LayoutElement>();

        if (targetText == null || layoutElement == null)
            return;

        Canvas.ForceUpdateCanvases();
        targetText.ForceMeshUpdate();

        float preferredWidth = targetText.GetPreferredValues(targetText.text, 9999f, 0f).x;
        layoutElement.preferredWidth = Mathf.Min(preferredWidth, maxWidth);
    }
}