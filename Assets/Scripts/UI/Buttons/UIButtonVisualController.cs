using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class UIButtonVisualController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private Sprite pressedIcon;

    [SerializeField] private TMP_Text label;
    [SerializeField] private Color defaultTextColor;
    [SerializeField] private Color pressedTextColor;

    private void OnEnable()
    {
        ApplyDefault();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ApplyPressed();

        // 🔥 автоматический возврат через 1 кадр
        StartCoroutine(ResetNextFrame());
    }

    private System.Collections.IEnumerator ResetNextFrame()
    {
        yield return null;
        ApplyDefault();
    }

    private void ApplyPressed()
    {
        if (icon != null)
            icon.sprite = pressedIcon;

        if (label != null)
        {
            Color c = pressedTextColor;
            c.a = 1f;
            label.color = c;
        }
    }

    private void ApplyDefault()
    {
        if (icon != null)
            icon.sprite = defaultIcon;

        if (label != null)
        {
            Color c = defaultTextColor;
            c.a = 1f;
            label.color = c;
        }
    }
}