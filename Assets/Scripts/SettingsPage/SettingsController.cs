using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
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

    [Header("Colors")]
    [SerializeField] private Color selectedButtonColor = new Color32(216, 206, 240, 255);
    [SerializeField] private Color unselectedButtonColor = new Color32(255, 255, 255, 0);
    [SerializeField] private Color selectedTextColor = new Color32(143, 121, 198, 255);
    [SerializeField] private Color unselectedTextColor = new Color32(55, 49, 73, 255);

    [SerializeField] private ConfirmPopup confirmPopup;

    private const string MusicVolumeKey = "music_volume";
    private const string SfxVolumeKey = "sfx_volume";

    private void Start()
    {
        InitializeUI();
        LoadSettings();
        BindButtons();
        BindSliders();
        RefreshLanguageUI();
        RefreshAudioUI();
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(Language lang)
    {
        RefreshLanguageUI();
    }

    // ================= INIT =================

    private void InitializeUI()
    {
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
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.4f);

        if (musicSlider != null)
            musicSlider.value = musicVolume;

        if (sfxSlider != null)
            sfxSlider.value = sfxVolume;

        ApplyMusicVolume(musicVolume, false);
        ApplySfxVolume(sfxVolume, false);
    }

    // ================= BUTTONS =================

    private void BindButtons()
    {
        if (enButton != null)
        {
            enButton.onClick.RemoveAllListeners();
            enButton.onClick.AddListener(() =>
            {
                LocalizationManager.Instance.SetLanguage(Language.English);
            });
        }

        if (ruButton != null)
        {
            ruButton.onClick.RemoveAllListeners();
            ruButton.onClick.AddListener(() =>
            {
                LocalizationManager.Instance.SetLanguage(Language.Russian);
            });
        }

        if (kgButton != null)
        {
            kgButton.onClick.RemoveAllListeners();
            kgButton.onClick.AddListener(() =>
            {
                LocalizationManager.Instance.SetLanguage(Language.Kyrgyz);
            });
        }

        if (restartEpisodeButton != null)
        {
            restartEpisodeButton.onClick.RemoveAllListeners();
            restartEpisodeButton.onClick.AddListener(OnRestartEpisodePressed);
        }

        if (resetGameButton != null)
        {
            resetGameButton.onClick.RemoveAllListeners();
            resetGameButton.onClick.AddListener(OnResetGamePressed);
        }
    }

    private void BindSliders()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
    }

    // ================= AUDIO =================

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
        if (save)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    private void ApplySfxVolume(float value, bool save)
    {
        if (save)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    // ================= UI =================

    private void RefreshLanguageUI()
    {
        if (LocalizationManager.Instance == null)
            return;

        var lang = LocalizationManager.Instance.CurrentLanguage;

        UpdateLanguageButton(enButtonBackground, enButtonText, lang == Language.English);
        UpdateLanguageButton(kgButtonBackground, kgButtonText, lang == Language.Kyrgyz);
        UpdateLanguageButton(ruButtonBackground, ruButtonText, lang == Language.Russian);
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

    // ================= ACTIONS =================

    private void OnRestartEpisodePressed()
    {
        if (confirmPopup == null)
            return;

        confirmPopup.Show(
            "restart_confirm",
            () =>
            {
                bool restarted = SaveSystem.RestartCurrentEpisode();

                if (!restarted)
                    return;

                if (StatSystem.Instance != null)
                    StatSystem.Instance.ResetEpisodeStats();
            }
        );
    }

    private void OnResetGamePressed()
    {
        if (confirmPopup == null)
            return;

        confirmPopup.Show(
            "reset_confirm",
            () =>
            {
                SaveSystem.Clear();
            }
        );
    }
}