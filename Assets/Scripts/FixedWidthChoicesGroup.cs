using UnityEngine;
using UnityEngine.UI;

public class FixedWidthChoicesGroup : MonoBehaviour
{
    public Button[] choiceButtons;

    [Header("Фиксированный размер кнопок")]
    public float width = 815f;
    public float height = 100f;

    [Header("Расстояние между кнопками (в пикселях)")]
    public float spacing = 5f; // ~пара сантиметров на экране

    private VerticalLayoutGroup layoutGroup;

    void Awake()
    {
        layoutGroup = GetComponent<VerticalLayoutGroup>();

        ApplySpacing();
        ApplySize();
    }

    public void ApplySpacing()
    {
        if (layoutGroup != null)
        {
            layoutGroup.spacing = spacing;
        }
        else
        {
            Debug.LogWarning("FixedWidthChoicesGroup: Нет VerticalLayoutGroup на объекте!");
        }
    }

    public void ApplySize()
    {
        foreach (var btn in choiceButtons)
        {
            if (btn == null) continue;

            RectTransform rect = btn.GetComponent<RectTransform>();
            if (rect == null) continue;

            float targetHeight = (height > 0f) ? height : rect.sizeDelta.y;
            rect.sizeDelta = new Vector2(width, targetHeight);
        }
    }
}
