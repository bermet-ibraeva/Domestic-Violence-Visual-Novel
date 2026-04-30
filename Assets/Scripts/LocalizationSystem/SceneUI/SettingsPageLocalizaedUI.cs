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

        if (titleText != null)
            titleText.text = L("title");

        if (languageLabelText != null)
            languageLabelText.text = L("language");

        if (audioLabelText != null)
            audioLabelText.text = L("audio");

        if (progressLabelText != null)
            progressLabelText.text = L("progress");

        if (musicLabelText != null)
            musicLabelText.text = L("music");

        if (sfxLabelText != null)
            sfxLabelText.text = L("sfx");

        if (restartEpisodeText != null)
            restartEpisodeText.text = L("restart_episode");

        if (newGameText != null)
            newGameText.text = L("new_game");
    }

    string L(string key)
    {
        return LocalizationManager.Instance.GetText("SettingsPage", key);
    }
}