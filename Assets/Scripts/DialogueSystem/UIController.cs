using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public GameObject authorPanel;
    public TextMeshProUGUI authorText;

    public GameObject ainazPanel;
    public TextMeshProUGUI ainazName;
    public TextMeshProUGUI ainazText;

    public GameObject otherPanel;
    public TextMeshProUGUI otherName;
    public TextMeshProUGUI otherText;

    public GameObject choicesPanel;
    public ChoiceButton[] choiceButtons; // твои кнопки

    public void HideAll()
    {
        authorPanel.SetActive(false);
        ainazPanel.SetActive(false);
        otherPanel.SetActive(false);
        choicesPanel.SetActive(false);
    }
}
