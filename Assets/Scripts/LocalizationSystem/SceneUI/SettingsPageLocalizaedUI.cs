using TMPro;
using UnityEngine;
using System.Collections;

public class SettingsPageLocalizedUI : MonoBehaviour
{
    [Header("Titles")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text languageLabelText;
    [SerializeField] private TMP_Text audioLabelText;
    [SerializeField] private TMP_Text progressLabelText;

    [Header("Audio")]
    [SerializeField] private TMP_Text musicLabelText;
    [SerializeField] private TMP_Text sfxLabelText;

    [Header("Buttons")]
    [SerializeField] private TMP_Text restartEpisodeText;
    [SerializeField] private TMP_Text newGameText;

    private void OnEnable()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
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