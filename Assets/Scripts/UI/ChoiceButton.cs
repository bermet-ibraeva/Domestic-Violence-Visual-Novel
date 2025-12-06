using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ChoiceButton : MonoBehaviour
{
    [Header("Auto-detected")]
    public Button button;
    public TextMeshProUGUI text;

    private Action onClick;

    void Awake()
    {
        // Автопоиск кнопки, если ты забыл её назначить
        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
                Debug.LogError($"[ChoiceButton] На объекте {gameObject.name} нет компонента Button!");
        }

        // Автопоиск текста (во всех дочерних объектах)
        if (text == null)
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
            if (text == null)
                Debug.LogError($"[ChoiceButton] На кнопке {gameObject.name} нет TextMeshProUGUI!");
        }

        // Подписка на клик
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                onClick?.Invoke();
            });
        }
    }

    public void SetText(string value)
    {
        if (text != null)
            text.text = value;
    }

    public void SetCallback(Action callback)
    {
        onClick = callback;
    }
}
