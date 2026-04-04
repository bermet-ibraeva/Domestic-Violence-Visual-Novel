using TMPro;
using UnityEngine;

public class MainMenuLocalizedUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private TMP_Text playText;
    [SerializeField] private TMP_Text continueText;
    [SerializeField] private TMP_Text settingsText;
    [SerializeField] private TMP_Text aboutText;
    [SerializeField] private TMP_Text notesText;

    [Header("Episode Title")]
    [SerializeField] private TMP_Text episodeTitleText;

    [Header("Episode Settings")]
    [SerializeField] private int currentEpisodeNumber = 1;

    private void Start()
    {
        RefreshUI();

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(Language language)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (LocalizationManager.Instance == null)
            return;

        playText.text = LocalizationManager.Instance.GetText("MainMenu", "play");
        continueText.text = LocalizationManager.Instance.GetText("MainMenu", "continue");
        settingsText.text = LocalizationManager.Instance.GetText("MainMenu", "settings");
        aboutText.text = LocalizationManager.Instance.GetText("MainMenu", "about");
        notesText.text = LocalizationManager.Instance.GetText("MainMenu", "notes");

        string episodeKey = $"episode_{currentEpisodeNumber}_title";
        episodeTitleText.text = LocalizationManager.Instance.GetText("MainMenu", episodeKey);
    }
}