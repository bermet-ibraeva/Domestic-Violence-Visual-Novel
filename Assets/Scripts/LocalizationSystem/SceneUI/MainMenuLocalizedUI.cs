using TMPro;
using UnityEngine;
using System.Collections;

public class MainMenuLocalizedUI : MonoBehaviour
{
    [Header("Static Buttons")]
    [SerializeField] private TMP_Text settingsText;
    [SerializeField] private TMP_Text aboutText;
    [SerializeField] private TMP_Text notesText;

    private void OnEnable()
    {
        StartCoroutine(WaitForLocalization());
    }

    private IEnumerator WaitForLocalization()
    {
        while (LocalizationManager.Instance == null || !LocalizationManager.Instance.IsLoaded)
            yield return null;

        RefreshUI();

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

        if (aboutText != null)
            aboutText.text = LocalizationManager.Instance.GetText("MainMenu", "about");

        if (notesText != null)
            notesText.text = LocalizationManager.Instance.GetText("MainMenu", "notes");

        if (settingsText != null)
            settingsText.text = LocalizationManager.Instance.GetText("MainMenu", "settings");

    }   
}