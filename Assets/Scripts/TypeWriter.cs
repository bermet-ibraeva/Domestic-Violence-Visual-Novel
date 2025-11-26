using System.Collections;
using TMPro;
using UnityEngine;

public static class TypewriterEffect
{
    // Эффект печати для любого TextMeshProUGUI
    public static IEnumerator ShowText(TextMeshProUGUI textUI, string fullText, float charDelay = 0.02f)
    {
        textUI.text = "";
        foreach (char c in fullText)
        {
            textUI.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }
}
