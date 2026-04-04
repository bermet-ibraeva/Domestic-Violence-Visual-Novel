using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private Button backButton;
    [SerializeField] private string backSceneName = "MainMenu";
    [SerializeField] private string episodeSceneName = "EpisodePage";


    [Header("Language Buttons")]
    [SerializeField] private Button enButton;
    [SerializeField] private Button kgButton;
    [SerializeField] private Button ruButton;

    [Header("Language Button Visuals")]
    [SerializeField] private Image enButtonBackground;
    [SerializeField] private Image kgButtonBackground;
    [SerializeField] private Image ruButtonBackground;

    [SerializeField] private TMP_Text enButtonText;
    [SerializeField] private TMP_Text kgButtonText;
    [SerializeField] private TMP_Text ruButtonText;

    [Header("Audio")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text musicPercentText;
    [SerializeField] private TMP_Text sfxPercentText;

    [Header("Progress Buttons")]
    [SerializeField] private Button restartEpisodeButton;
    [SerializeField] private Button resetGameButton;

    [Header("Optional Confirmation Panels")]
    [SerializeField] private GameObject restartEpisodeConfirmPanel;
    [SerializeField] private GameObject resetGameConfirmPanel;
    [SerializeField] private Button confirmRestartEpisodeButton;
    [SerializeField] private Button cancelRestartEpisodeButton;
    [SerializeField] private Button confirmResetGameButton;
    [SerializeField] private Button cancelResetGameButton;

    [Header("Colors")]
    [SerializeField] private Color selectedButtonColor = new Color32(216, 206, 240, 255);
    [SerializeField] private Color unselectedButtonColor = new Color32(255, 255, 255, 0);
    [SerializeField] private Color selectedTextColor = new Color32(143, 121, 198, 255);
    [SerializeField] private Color unselectedTextColor = new Color32(55, 49, 73, 255);

    private const string LanguageKey = "language";
    private const string MusicVolumeKey = "music_volume";
    private const string SfxVolumeKey = "sfx_volume";

    private string currentLanguage = "KG";

    private void Start()
    {
        InitializeUI();
        LoadSettings();
        BindButtons();
        BindSliders();
        RefreshLanguageUI();
        RefreshAudioUI();
    }

    private void InitializeUI()
    {
        if (restartEpisodeConfirmPanel != null)
            restartEpisodeConfirmPanel.SetActive(false);

        if (resetGameConfirmPanel != null)
            resetGameConfirmPanel.SetActive(false);

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
        }
    }

    private void LoadSettings()
    {
        currentLanguage = PlayerPrefs.GetString(LanguageKey, "KG");

        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.4f);

        if (musicSlider != null)
            musicSlider.value = musicVolume;

        if (sfxSlider != null)
            sfxSlider.value = sfxVolume;

        ApplyLanguage(currentLanguage, false);
        ApplyMusicVolume(musicVolume, false);
        ApplySfxVolume(sfxVolume, false);
    }

    private void BindButtons()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackPressed);

        if (enButton != null)
            enButton.onClick.AddListener(() => OnLanguageSelected("EN"));

        if (kgButton != null)
            kgButton.onClick.AddListener(() => OnLanguageSelected("KG"));

        if (ruButton != null)
            ruButton.onClick.AddListener(() => OnLanguageSelected("RU"));

        if (restartEpisodeButton != null)
            restartEpisodeButton.onClick.AddListener(OnRestartEpisodePressed);

        if (resetGameButton != null)
            resetGameButton.onClick.AddListener(OnResetGamePressed);

        if (confirmRestartEpisodeButton != null)
            confirmRestartEpisodeButton.onClick.AddListener(ConfirmRestartEpisode);

        if (cancelRestartEpisodeButton != null)
            cancelRestartEpisodeButton.onClick.AddListener(CloseRestartEpisodePanel);

        if (confirmResetGameButton != null)
            confirmResetGameButton.onClick.AddListener(ConfirmResetGame);

        if (cancelResetGameButton != null)
            cancelResetGameButton.onClick.AddListener(CloseResetGamePanel);
    }

    private void BindSliders()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
    }

    private void OnBackPressed()
    {
        SceneManager.LoadScene(backSceneName);
    }

    private void OnLanguageSelected(string languageCode)
    {
        ApplyLanguage(languageCode, true);
        RefreshLanguageUI();
    }

    private void ApplyLanguage(string languageCode, bool save)
    {
        currentLanguage = languageCode;

        // Здесь подключишь свою локализацию
        // Например:
        // LocalizationManager.Instance.SetLanguage(languageCode);

        if (save)
        {
            PlayerPrefs.SetString(LanguageKey, languageCode);
            PlayerPrefs.Save();
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        ApplyMusicVolume(value, true);
        RefreshAudioUI();
    }

    private void OnSfxSliderChanged(float value)
    {
        ApplySfxVolume(value, true);
        RefreshAudioUI();
    }

    private void ApplyMusicVolume(float value, bool save)
    {
        // Здесь подключишь свой AudioManager
        // Например:
        // AudioManager.Instance.SetMusicVolume(value);

        if (save)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    private void ApplySfxVolume(float value, bool save)
    {
        // Здесь подключишь свой AudioManager
        // Например:
        // AudioManager.Instance.SetSfxVolume(value);

        if (save)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    private void RefreshLanguageUI()
    {
        UpdateLanguageButton(enButtonBackground, enButtonText, currentLanguage == "EN");
        UpdateLanguageButton(kgButtonBackground, kgButtonText, currentLanguage == "KG");
        UpdateLanguageButton(ruButtonBackground, ruButtonText, currentLanguage == "RU");
    }

    private void UpdateLanguageButton(Image background, TMP_Text label, bool isSelected)
    {
        if (background != null)
            background.color = isSelected ? selectedButtonColor : unselectedButtonColor;

        if (label != null)
            label.color = isSelected ? selectedTextColor : unselectedTextColor;
    }

    private void RefreshAudioUI()
    {
        if (musicPercentText != null && musicSlider != null)
            musicPercentText.text = Mathf.RoundToInt(musicSlider.value * 100f) + "%";

        if (sfxPercentText != null && sfxSlider != null)
            sfxPercentText.text = Mathf.RoundToInt(sfxSlider.value * 100f) + "%";
    }


    public void OnRestartEpisodePressed()
    {
        bool restarted = SaveSystem.RestartCurrentEpisode();

        if (!restarted)
            return;

        if (StatSystem.Instance != null)
            StatSystem.Instance.ResetEpisodeStats();

        SceneManager.LoadScene(episodeSceneName);
    }

    private void OnResetGamePressed()
    {
        if (resetGameConfirmPanel != null)
        {
            resetGameConfirmPanel.SetActive(true);
        }
        else
        {
            ConfirmResetGame();
        }
    }

    private void CloseRestartEpisodePanel()
    {
        if (restartEpisodeConfirmPanel != null)
            restartEpisodeConfirmPanel.SetActive(false);
    }

    private void CloseResetGamePanel()
    {
        if (resetGameConfirmPanel != null)
            resetGameConfirmPanel.SetActive(false);
    }

    private void ConfirmRestartEpisode()
    {
        bool restarted = SaveSystem.RestartCurrentEpisode();

        if (!restarted)
            return;

        StatSystem.Instance.ResetEpisodeStats();

        SceneManager.LoadScene("EpisodePage");
    }

    private void ConfirmResetGame()
    {
        CloseResetGamePanel();

        // Полный сброс игры
        // Лучше не делать просто PlayerPrefs.DeleteAll(),
        // если у тебя там и язык, и звук тоже хранятся.
        //
        // Ниже вариант: удаляем только прогресс игры,
        // а настройки можно оставить.

        // Пример:
        // SaveSystem.DeleteSave();

        // Если хочешь и настройки тоже сбрасывать:
        // PlayerPrefs.DeleteAll();

        // Временный вариант:
        PlayerPrefs.DeleteKey("episode_path");
        PlayerPrefs.DeleteKey("current_node_id");
        PlayerPrefs.DeleteKey("chapter_number");
        PlayerPrefs.DeleteKey("sparks_total");
        PlayerPrefs.DeleteKey("trust_ag_total");
        PlayerPrefs.DeleteKey("trust_ja_total");
        PlayerPrefs.DeleteKey("risk_total");
        PlayerPrefs.DeleteKey("safety_total");
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMenu");
    }
}