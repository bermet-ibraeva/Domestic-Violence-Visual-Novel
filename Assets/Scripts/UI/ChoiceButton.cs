using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ChoiceButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI text;

    private Action onClick;

    void Awake()
    {
        button.onClick.AddListener(() =>
        {
            onClick?.Invoke();
        });
    }

    public void SetText(string value)
    {
        text.text = value;
    }

    public void SetCallback(Action callback)
    {
        onClick = callback;
    }
}
