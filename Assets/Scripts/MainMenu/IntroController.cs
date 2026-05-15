using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroController : MonoBehaviour
{
    [System.Serializable]
    public class IntroPage
    {
        public GameObject pageObject;
        public Button pageButton;
        public TMP_Text pageText;
        public string localizationKey;
    }

    [Header("Roots")]
    [SerializeField] private GameObject introRoot;

    [SerializeField] private GameObject mainMenuRoot;

    [Header("Pages")]
    [SerializeField] private GameObject languagePage;

    [SerializeField] private List<IntroPage> introPages;

    [Header("Language UI")]
    [SerializeField] private TMP_Text languageTitleText;

    [Header("Language Buttons")]
    [SerializeField] private Button kyrgyzButton;
    [SerializeField] private Button russianButton;
    [SerializeField] private Button englishButton;
    [SerializeField] private TMP_Text kyrgyzButtonText;
    [SerializeField] private TMP_Text russianButtonText;
    [SerializeField] private TMP_Text englishButtonText;

    [Header("Radio Icons")]
    [SerializeField] private Image kyrgyzRadioIcon;
    [SerializeField] private Image russianRadioIcon;

    [SerializeField] private Image englishRadioIcon;

    [Header("Radio Sprites")]
    [SerializeField] private Sprite selectedRadioSprite;

    [SerializeField] private Sprite unselectedRadioSprite;

    [Header("Page Buttons")]
    [SerializeField] private Button languageNextButton;
    [SerializeField] private TMP_Text languageNextButtonText;
    [SerializeField] private TMP_Text page1nextButtonText;
    [SerializeField] private TMP_Text startGameButtonText;

    private int currentPage = -1;

    private bool languageSelected = false;

    private Language selectedLanguage;

    private void Start()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("[Intro] LocalizationManager is NULL");
            return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[Intro] SaveManager is NULL");
            return;
        }

        if (SaveManager.Instance.Data.introCompleted)
        {
            introRoot.SetActive(false);

            mainMenuRoot.SetActive(true);

            return;
        }

        mainMenuRoot.SetActive(false);

        SetupLanguageButtons();

        SetupPageButtons();

        languageNextButton.interactable = false;

        languageNextButton.onClick.AddListener(OnLanguageNextClicked);

        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;

        // Current saved language
        selectedLanguage =
            LocalizationManager.Instance.CurrentLanguage;

        // If user already selected language before
        languageSelected =
            LocalizationManager.Instance.HasSelectedLanguage;

        languageNextButton.interactable =
            languageSelected;

        UpdateLanguageSelectionUI();

        RefreshLocalization();

        ShowLanguagePage();
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -=
                OnLanguageChanged;
        }
    }

    // =========================
    // LANGUAGE
    // =========================

    private void SetupLanguageButtons()
    {
        kyrgyzButton.onClick.AddListener(() =>
        {
            SelectLanguage(Language.Kyrgyz);
        });

        russianButton.onClick.AddListener(() =>
        {
            SelectLanguage(Language.Russian);
        });

        englishButton.onClick.AddListener(() =>
        {
            SelectLanguage(Language.English);
        });

    }

    private void SelectLanguage(Language language)
    {
        if (LocalizationManager.Instance == null)
            return;

        languageSelected = true;

        languageNextButton.interactable = true;

        selectedLanguage = language;

        LocalizationManager.Instance.SetLanguage(language);

        UpdateLanguageSelectionUI();
    }

    private void UpdateLanguageSelectionUI()
    {
        if (kyrgyzRadioIcon != null)
        {
            kyrgyzRadioIcon.sprite =
                selectedLanguage == Language.Kyrgyz
                ? selectedRadioSprite
                : unselectedRadioSprite;
        }

        if (russianRadioIcon != null)
        {
            russianRadioIcon.sprite =
                selectedLanguage == Language.Russian
                ? selectedRadioSprite
                : unselectedRadioSprite;
        }

        if (englishRadioIcon != null)
        {
            englishRadioIcon.sprite =
                selectedLanguage == Language.English
                ? selectedRadioSprite
                : unselectedRadioSprite;
        }
    }

    private void OnLanguageNextClicked()
    {
        if (!languageSelected)
            return;

        currentPage = 0;

        ShowPage(currentPage);
    }

    // =========================
    // PAGE BUTTONS
    // =========================

    private void SetupPageButtons()
    {
        for (int i = 0; i < introPages.Count; i++)
        {
            IntroPage page = introPages[i];

            if (page.pageButton == null)
                continue;

            int pageIndex = i;

            page.pageButton.onClick.RemoveAllListeners();

            if (pageIndex >= introPages.Count - 1)
            {
                page.pageButton.onClick.AddListener(StartGame);
            }
            else
            {
                page.pageButton.onClick.AddListener(NextPage);
            }
        }
    }

    // =========================
    // FLOW
    // =========================

    private void NextPage()
    {
        if (currentPage >= introPages.Count - 1)
            return;

        currentPage++;

        ShowPage(currentPage);
    }

    private void StartGame()
    {
        SaveManager.Instance.Data.introCompleted = true;

        SaveManager.Instance.Save();

        HideAllPages();

        introRoot.SetActive(false);

        mainMenuRoot.SetActive(true);
    }

    // =========================
    // UI
    // =========================

    private void ShowLanguagePage()
    {
        HideAllPages();

        languagePage.SetActive(true);
    }

    private void ShowPage(int index)
    {
        if (index < 0 || index >= introPages.Count)
            return;

        HideAllPages();

        introPages[index].pageObject.SetActive(true);
    }

    private void HideAllPages()
    {
        languagePage.SetActive(false);

        foreach (IntroPage page in introPages)
        {
            if (page.pageObject != null)
            {
                page.pageObject.SetActive(false);
            }
        }
    }

    // =========================
    // LOCALIZATION
    // =========================

    private void OnLanguageChanged(Language language)
    {
        RefreshLocalization();
    }

    private void RefreshLocalization()
    {
        if (LocalizationManager.Instance == null)
            return;

        if (languageTitleText != null)
        {
            languageTitleText.text =
                TextFormatter.Format(
                    L("language_select_title")
                );
        }

        // Language buttons
        if (kyrgyzButtonText != null)
        {
            kyrgyzButtonText.text = "Кыргызча";
        }

        if (russianButtonText != null)
        {
            russianButtonText.text = "Русский";
        }

        if (englishButtonText != null)
        {
            englishButtonText.text = "English";
        }

        if (languageNextButtonText != null)
        {
            languageNextButtonText.GetComponentInChildren<TMP_Text>().text =
                TextFormatter.Format(
                    L("next")
                );
        }

        // First page NEXT button
        if (page1nextButtonText != null)
        {
            page1nextButtonText.text =
                TextFormatter.Format(
                    L("next")
                );
        }

        // Second page START button
        if (startGameButtonText != null)
        {
            startGameButtonText.text =
                TextFormatter.Format(
                    L("start_game")
                );
        }

        foreach (IntroPage page in introPages)
        {
            if (page.pageText == null)
                continue;

            if (string.IsNullOrEmpty(page.localizationKey))
                continue;

            page.pageText.text =
                TextFormatter.Format(
                    L(page.localizationKey)
                );
        }
    }

    private string L(string key)
    {
        if (LocalizationManager.Instance == null)
            return key;

        return LocalizationManager.Instance
            .GetText("MainMenu", key);
    }
}