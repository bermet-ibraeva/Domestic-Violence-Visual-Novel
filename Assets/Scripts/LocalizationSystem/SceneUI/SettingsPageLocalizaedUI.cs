using TMPro;
using UnityEngine;

public class SettingsPageLocalizedUI : MonoBehaviour
{
    [Header("Titles")]
    [SerializeField] private TMP_Text titleText;           // Настройки
    [SerializeField] private TMP_Text languageLabelText;   // Язык
    [SerializeField] private TMP_Text audioLabelText;      // Аудио
    [SerializeField] private TMP_Text progressLabelText;   // Прогресс игры

    [Header("Audio")]
    [SerializeField] private TMP_Text musicLabelText;      // Фоновая музыка
    [SerializeField] private TMP_Text sfxLabelText;        // Звук в игре

    [Header("Buttons")]
    [SerializeField] private TMP_Text restartEpisodeText;  // Перезапустить эпизод
    [SerializeField] private TMP_Text newGameText;         // Начать игру заново

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

    private void HandleLanguageChanged(Language lang)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (LocalizationManager.Instance == null)
            return;

        // Заголовки
        titleText.text = LocalizationManager.Instance.GetText("SettingsPage", "title");
        languageLabelText.text = LocalizationManager.Instance.GetText("SettingsPage", "language");
        audioLabelText.text = LocalizationManager.Instance.GetText("SettingsPage", "audio");
        progressLabelText.text = LocalizationManager.Instance.GetText("SettingsPage", "progress");

        // Аудио
        musicLabelText.text = LocalizationManager.Instance.GetText("SettingsPage", "music");
        sfxLabelText.text = LocalizationManager.Instance.GetText("SettingsPage", "sfx");

        // Кнопки
        restartEpisodeText.text = LocalizationManager.Instance.GetText("SettingsPage", "restart_episode");
        newGameText.text = LocalizationManager.Instance.GetText("SettingsPage", "new_game");
    }
}