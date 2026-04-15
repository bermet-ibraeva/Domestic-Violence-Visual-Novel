using UnityEngine;
using TMPro;


// used for author panel and choice panel
public class SimpleCenterPanel : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public float baseHeight = 150f;
    public float padding = 40f;

    public void RefreshSize()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null || targetText == null) return;

        Canvas.ForceUpdateCanvases();
        targetText.ForceMeshUpdate();

        float preferredHeight = targetText.preferredHeight;
        float h = Mathf.Max(baseHeight, preferredHeight + padding);

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }
}