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
    private SimpleCenterPanel simplePanel;

    void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
                Debug.LogError($"[ChoiceButton] На объекте {gameObject.name} нет компонента Button!");
        }

        if (text == null)
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
            if (text == null)
                Debug.LogError($"[ChoiceButton] На кнопке {gameObject.name} нет TextMeshProUGUI!");
        }

        simplePanel = GetComponent<SimpleCenterPanel>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        onClick?.Invoke();
    }

    public void SetText(string value)
    {
        if (text != null)
            text.text = value;

        var panel = GetComponent<SimpleCenterPanel>();
        if (panel != null)
            panel.RefreshSize();
    }

    public void SetCallback(Action callback)
    {
        onClick = callback;
    }
}