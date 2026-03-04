using UnityEngine;
using TMPro;

public class CharacterDialoguePanel : MonoBehaviour
{
    [Header("UI Links")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public RectTransform namePlate;
    
    [Header("Settings")]
    public float topPadding = 200f; // Тот самый отступ под шапку внутри панели
    public float pricePerLine = 40f;
    public float minHeight = 280f;

    public void SetDialogue(string name, string text)
    {
        if (nameText != null) nameText.text = name;
        if (dialogueText != null) dialogueText.text = text;

        if (namePlate != null) 
            namePlate.gameObject.SetActive(!string.IsNullOrEmpty(name));

        RefreshSize();
    }

    public void RefreshSize()
    {
        // 1. Проверка ссылок, чтобы не было NullReferenceException
        if (dialogueText == null) 
        {
            Debug.LogError($"На объекте {gameObject.name} не назначена ссылка на Dialogue Text!");
            return;
        }

        RectTransform rect = GetComponent<RectTransform>();
        RectTransform tRect = dialogueText.GetComponent<RectTransform>();

        // 2. Расчет текста
        tRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
        dialogueText.ForceMeshUpdate();

        // 3. Считаем высоту
        int lines = Mathf.Max(1, dialogueText.textInfo.lineCount);
        float h = topPadding + (lines * pricePerLine);
        
        if (h < minHeight) h = minHeight;

        // 4. Применяем высоту (раз Pivot Y = 1, панель растет вниз)
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        
        // 5. Позиционируем текст. Если Pos Y был не 0, теперь ставим его четко под отступ.
        tRect.anchoredPosition = new Vector2(0, -topPadding);
    }
}