using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class CharacterDialoguePanel : MonoBehaviour
{
    [Header("Текст диалога")]
    public TextMeshProUGUI dialogueText;
    
    [Header("Плашка имени")]
    public RectTransform namePlate;
    public TextMeshProUGUI nameText;

    [Header("Настройки Border (Шапки)")]
    public float headerOffset = 200f; // Те самые 200px сверху
    public float baseHeight = 250f;
    public float pricePerLine = 50f;

    private RectTransform rect;
    private RectTransform textRect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (dialogueText != null) textRect = dialogueText.GetComponent<RectTransform>();
    }

    public void SetDialogue(string name, string text)
    {
        if (nameText != null) nameText.text = name;
        if (dialogueText != null) dialogueText.text = text;
        
        // Включаем/выключаем плашку имени, если имени нет
        if (namePlate != null) namePlate.gameObject.SetActive(!string.IsNullOrEmpty(name));

        RefreshSize();
    }

    public void RefreshSize()
    {
        if (rect == null || dialogueText == null) return;

        // 1. Подготавливаем ширину
        float currentWidth = rect.rect.width;
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
        dialogueText.ForceMeshUpdate();

        // 2. Считаем высоту (Шапка + Строки)
        int lineCount = Mathf.Max(1, dialogueText.textInfo.lineCount);
        float targetHeight = headerOffset + (lineCount * pricePerLine);

        // 3. Применяем размер панели (Pivot Y должен быть 1 в инспекторе!)
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        // 4. Позиционируем текст СТРОГО под шапкой
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 1);
        textRect.anchoredPosition = new Vector2(0, -headerOffset);
        
        // 5. Если есть плашка имени, можно ее тоже подвинуть (опционально)
        // Например, прижать к самому верху
        if (namePlate != null)
        {
            namePlate.anchoredPosition = new Vector2(namePlate.anchoredPosition.x, 0);
        }
    }
}