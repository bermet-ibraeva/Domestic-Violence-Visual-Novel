using UnityEngine;

public class LayoutController : MonoBehaviour
{
    [Header("Dialogue Panels")]
    public AdaptivePanel AuthorPanel;
    public AdaptivePanel LeftCharacterPanel;
    public AdaptivePanel RightCharacterPanel;

    public void ApplyLayout(string character)
    {
        // Выключаем все
        AuthorPanel.gameObject.SetActive(false);
        LeftCharacterPanel.gameObject.SetActive(false);
        RightCharacterPanel.gameObject.SetActive(false);

        // Включаем нужную
        switch (character)
        {
            case "Narrator":
                AuthorPanel.gameObject.SetActive(true);
                AuthorPanel.RefreshSize();
                break;

            case "LeftCharacter":
                LeftCharacterPanel.gameObject.SetActive(true);
                LeftCharacterPanel.RefreshSize();
                break;

            case "RightCharacter":
                RightCharacterPanel.gameObject.SetActive(true);
                RightCharacterPanel.RefreshSize();
                break;
        }
    }
}