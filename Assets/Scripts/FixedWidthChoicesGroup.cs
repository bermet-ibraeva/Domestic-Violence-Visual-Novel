using UnityEngine;
using UnityEngine.UI;

public class FixedWidthChoicesGroup : MonoBehaviour
{
    public Button[] choiceButtons;

    [Header("Фиксированный размер кнопок")]
    public float width = 600f;
    public float height = 80f;  // если хочешь фикс высоту. Иначе оставь -1.

    void Awake()
    {
        ApplySize();
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
