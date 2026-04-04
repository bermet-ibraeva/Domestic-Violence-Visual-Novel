using UnityEngine;
using TMPro;

public class SafeTMPHandler : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;

    public void SetText(string newText)
    {
        if (dialogueText != null) // проверяем, не уничтожен ли TMP объект
        {
            dialogueText.text = newText;
        }
        else
        {
            Debug.LogWarning("TMP object destroyed or missing!");
        }
    }
}
