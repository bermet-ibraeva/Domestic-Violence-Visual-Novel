using UnityEngine;
using TMPro;

public static class DialoguePositioner
{
    // Устанавливает позицию текста в зависимости от персонажа
    public static void SetPosition(TextMeshProUGUI textUI, string character)
    {
        RectTransform rect = textUI.GetComponent<RectTransform>();

        if(character == "Айназ")
        {
            rect.anchorMin = new Vector2(0.7f, 0.3f); // справа
            rect.anchorMax = new Vector2(0.95f, 0.6f);
            rect.anchoredPosition = Vector2.zero;
        }
        else
        {
            rect.anchorMin = new Vector2(0.3f, 0.3f); // по центру
            rect.anchorMax = new Vector2(0.7f, 0.6f);
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
