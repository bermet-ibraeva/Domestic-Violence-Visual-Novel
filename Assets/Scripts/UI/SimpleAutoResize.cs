using UnityEngine;
using TMPro;

public class SimpleAutoResize : MonoBehaviour
{
    public TextMeshProUGUI textUI;

    public float topOffset = 75f;   // расстояние от верха панели
    public float bottomPadding = 30f;

    public float minHeight = 200f;
    public float maxHeight = 500f;

    private RectTransform panelRect;
    private RectTransform textRect;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        textRect = textUI.GetComponent<RectTransform>();
    }

    public void SetText(string text)
    {
        textUI.text = text;

        Canvas.ForceUpdateCanvases();
        textUI.ForceMeshUpdate();

        float width = textRect.rect.width;
        float height = textUI.GetPreferredValues(text, width, 0).y;

        // верх фиксирован (pivot сверху)
        textRect.offsetMax = new Vector2(textRect.offsetMax.x, -topOffset);

        // низ уходит вниз
        textRect.offsetMin = new Vector2(textRect.offsetMin.x, -(topOffset + height));

        // панель растёт вниз (pivot = 1)
        float panelHeight = topOffset + height + bottomPadding;
        panelHeight = Mathf.Clamp(panelHeight, minHeight, maxHeight);

        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelHeight);
    }
}