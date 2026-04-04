using TMPro;
using UnityEngine;

public class NamePlateAutoWidth : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public float padding = 40f;
    public float minWidth = 300f;
    public float maxWidth = 500f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void SetName(string name)
    {
        nameText.text = name;
        nameText.ForceMeshUpdate();

        float width = nameText.preferredWidth + padding;
        width = Mathf.Clamp(width, minWidth, maxWidth);

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}