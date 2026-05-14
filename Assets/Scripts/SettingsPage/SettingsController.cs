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
    [SerializeField] private Slider sceneMusicSlider;

    [SerializeField] private TMP_Text musicPercentText;
    [SerializeField] private TMP_Text sceneMusicPercentText;

    [Header("Progress Buttons")]
    [SerializeField] private Button restartEpisodeButton;
    [SerializeField] private Button resetGameButton;

    [Header("Colors")]
    [SerializeField] private Color selectedButtonColor =
        new Color32(216, 206, 240, 255);

    [SerializeField] private Color unselectedButtonColor =
        new Color32(255, 255, 255, 0);

    [SerializeField] private Color selectedTextColor =
        new Color32(143, 121, 198, 255);

    [SerializeField] private Color unselectedTextColor =
        new Color32(55, 49, 73, 255);

    [Header("Popup")]
    [SerializeField] private ConfirmPopup confirmPopup;

    // ================= INIT =================

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
        {
            LocalizationManager.Instance.OnLanguageChanged +=
                OnLanguageChanged;
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -=
                OnLanguageChanged;
        }
    }

    private void OnLanguageChanged(Language lang)
    {
        RefreshLanguageUI();
    }

    private void InitializeUI()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
        }

        if (sceneMusicSlider != null)
        {
            sceneMusicSlider.minValue = 0f;
            sceneMusicSlider.maxValue = 1f;
        }
    }

    private void LoadSettings()
    {
        float musicVolume = BackgroundMusicController.Instance != null ? BackgroundMusicController.Instance.GetVolume() : 1f;
        float sceneVolume = AudioManager.Instance != null ? AudioManager.Instance.SceneVolume : 1f;

        if (musicSlider != null)
            musicSlider.value = musicVolume;

        if (sceneMusicSlider != null)
            sceneMusicSlider.value = sceneVolume;

        ApplyMusicVolume(musicVolume);
        ApplySceneVolume(sceneVolume);
    }

    // ================= BUTTONS =================

    private void BindButtons()
    {
        if (enButton != null)
        {
            enButton.onClick.RemoveAllListeners();

            enButton.onClick.AddListener(() =>
            {
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.SetLanguage(
                        Language.English
                    );
                }
            });
        }

        if (ruButton != null)
        {
            ruButton.onClick.RemoveAllListeners();

            ruButton.onClick.AddListener(() =>
            {
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.SetLanguage(
                        Language.Russian
                    );
                }
            });
        }

        if (kgButton != null)
        {
            kgButton.onClick.RemoveAllListeners();

            kgButton.onClick.AddListener(() =>
            {
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.SetLanguage(
                        Language.Kyrgyz
                    );
                }
            });
        }

        if (restartEpisodeButton != null)
        {
            restartEpisodeButton.onClick.RemoveAllListeners();
            restartEpisodeButton.onClick.AddListener(
                OnRestartEpisodePressed
            );
        }

        if (resetGameButton != null)
        {
            resetGameButton.onClick.RemoveAllListeners();
            resetGameButton.onClick.AddListener(
                OnResetGamePressed
            );
        }
    }

    private void BindSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(
                OnMusicSliderChanged
            );
        }

        if (sceneMusicSlider != null)
        {
            sceneMusicSlider.onValueChanged.RemoveAllListeners();
            sceneMusicSlider.onValueChanged.AddListener(
                OnSceneMusicChanged
            );
        }
    }

    // ================= AUDIO =================

    private void OnMusicSliderChanged(float value)
    {
        ApplyMusicVolume(value);
        RefreshAudioUI();
    }

    private void OnSceneMusicChanged(float value)
    {
        ApplySceneVolume(value);
        RefreshAudioUI();
    }

    private void ApplyMusicVolume(float value)
    {
        if (BackgroundMusicController.Instance != null)
        {
            BackgroundMusicController.Instance.SetVolume(value);
        }
    }

    private void ApplySceneVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSceneVolume(value);
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    // ================= UI =================

    private void RefreshLanguageUI()
    {
        if (LocalizationManager.Instance == null)
            return;

        Language lang =
            LocalizationManager.Instance.CurrentLanguage;

        UpdateLanguageButton(
            enButtonBackground,
            enButtonText,
            lang == Language.English
        );

        UpdateLanguageButton(
            kgButtonBackground,
            kgButtonText,
            lang == Language.Kyrgyz
        );

        UpdateLanguageButton(
            ruButtonBackground,
            ruButtonText,
            lang == Language.Russian
        );
    }

    private void UpdateLanguageButton(
        Image background,
        TMP_Text label,
        bool isSelected
    )
    {
        if (background != null)
        {
            background.color =
                isSelected
                ? selectedButtonColor
                : unselectedButtonColor;
        }

        if (label != null)
        {
            label.color =
                isSelected
                ? selectedTextColor
                : unselectedTextColor;
        }
    }

    private void RefreshAudioUI()
    {
        if (musicPercentText != null && musicSlider != null)
        {
            musicPercentText.text = Mathf.RoundToInt(musicSlider.value * 100f) + "%";
        }

        if (sceneMusicPercentText != null && sceneMusicSlider != null)
        {
            sceneMusicPercentText.text = Mathf.RoundToInt(sceneMusicSlider.value * 100f) + "%";
        }
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
                if (SaveManager.Instance == null)
                {
                    Debug.LogError(
                        "[Settings] SaveManager is NULL"
                    );

                    return;
                }

                bool restarted =
                    SaveManager.Instance.RestartEpisode();

                if (!restarted)
                {
                    Debug.LogWarning(
                        "[Settings] Restart episode failed"
                    );

                    return;
                }

                if (StatSystem.Instance != null)
                {
                    StatSystem.Instance.ResetEpisodeStats();
                }

                Debug.Log(
                    "[Settings] Episode restarted"
                );
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
                if (SaveManager.Instance == null)
                {
                    Debug.LogError(
                        "[Settings] SaveManager is NULL"
                    );

                    return;
                }

                SaveManager.Instance.Clear();

                TempGameContext.ResetEpisode();

                if (StatSystem.Instance != null)
                {
                    StatSystem.Instance.ResetEpisodeStats();
                }

                Debug.Log(
                    "[Settings] Game reset completed"
                );
            }
        );
    }
}