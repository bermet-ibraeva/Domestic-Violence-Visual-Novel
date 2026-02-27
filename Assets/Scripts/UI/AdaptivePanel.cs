using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class AdaptivePanel : MonoBehaviour
{
    public TextMeshProUGUI targetText;

    [Header("Height Settings")]
    public float extraPadding = 30f;   // добавочный отступ сверху/снизу
    public float minHeight = 0f;
    public float maxHeight = 0f;       // 0 = без ограничения

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void RefreshSize()
    {
        if (targetText == null)
            return;

        targetText.ForceMeshUpdate();

        float width = rect.rect.width;

        Vector2 preferred = targetText.GetPreferredValues(
            targetText.text,
            width,
            0f
        );

        float newHeight = preferred.y + extraPadding;

        if (minHeight > 0f)
            newHeight = Mathf.Max(newHeight, minHeight);

        if (maxHeight > 0f)
            newHeight = Mathf.Min(newHeight, maxHeight);

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);
    }
}