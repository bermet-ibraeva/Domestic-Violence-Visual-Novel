using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI text;

    private string nextNode;
    private System.Action<string> callback;

    public void Init(string textValue, string next, System.Action<string> onClick)
    {
        text.text = textValue;
        nextNode = next;
        callback = onClick;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback?.Invoke(nextNode));
    }
}
