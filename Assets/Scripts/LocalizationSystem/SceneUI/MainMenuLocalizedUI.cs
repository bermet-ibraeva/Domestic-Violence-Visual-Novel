using TMPro;
using UnityEngine;

public class MainMenuLocalizedUI : MonoBehaviour
{
    [Header("Static Buttons")]
    [SerializeField] private TMP_Text settingsText;
    [SerializeField] private TMP_Text aboutText;
    [SerializeField] private TMP_Text notesText;

    private void OnEnable()
    {
        RefreshUI();

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(Language language)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (LocalizationManager.Instance == null)
            return;

        settingsText.text = LocalizationManager.Instance.GetText("MainMenu", "settings");
        aboutText.text = LocalizationManager.Instance.GetText("MainMenu", "about");
        notesText.text = LocalizationManager.Instance.GetText("MainMenu", "notes");
    }
}