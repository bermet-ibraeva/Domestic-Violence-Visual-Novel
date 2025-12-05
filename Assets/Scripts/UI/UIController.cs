using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Author UI")]
    public GameObject authorPanel;
    public TextMeshProUGUI authorText;

    [Header("Айназ UI")]
    public GameObject ainazPanel;
    public TextMeshProUGUI ainazName;
    public TextMeshProUGUI ainazText;

    [Header("Other character UI")]
    public GameObject otherPanel;
    public TextMeshProUGUI otherName;
    public TextMeshProUGUI otherText;

    [Header("Choices UI")]
    public GameObject choicesPanel;
    public ChoiceButton[] choiceButtons; // массив твоих кнопок


    // Скрыть ВСЁ перед новым показом
    public void HideAll()
    {
        authorPanel.SetActive(false);
        ainazPanel.SetActive(false);
        otherPanel.SetActive(false);
        choicesPanel.SetActive(false);
    }


    // Скрыть только кнопки выбора
    public void HideChoices()
    {
        choicesPanel.SetActive(false);

        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }


    // Показать кнопки выбора (любое количество)
    public void ShowChoices(System.Collections.Generic.List<Choice> choices, System.Action<string> callback)
    {
        if (choices == null || choices.Count == 0)
        {
            HideChoices();
            return;
        }

        choicesPanel.SetActive(true);

        // Сначала скрываем все кнопки
        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

        // Показываем столько, сколько есть в JSON
        for (int i = 0; i < choices.Count; i++)
        {
            if (i >= choiceButtons.Length)
            {
                Debug.LogWarning("UIController: больше выборов, чем кнопок!");
                break;
            }

            ChoiceButton button = choiceButtons[i];
            button.gameObject.SetActive(true);

            string nextNode = choices[i].nextNode;

            // Установка текста кнопки
            button.SetText(choices[i].text);

            // Установка действия
            button.SetCallback(() =>
            {
                callback?.Invoke(nextNode);
            });
        }
    }
}
