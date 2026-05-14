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
        public TMP_Text pageText;
        public string localizationKey;
    }

    [Header("Roots")]
    [SerializeField] private GameObject introRoot;
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private CanvasGroup introCanvasGroup;

    [Header("Pages")]
    [SerializeField] private GameObject languagePage;
    [SerializeField] private List<IntroPage> introPages;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button startButton;

    [Header("Button Texts")]
    [SerializeField] private TMP_Text nextButtonText;
    [SerializeField] private TMP_Text startButtonText;

    [Header("Language Buttons")]
    [SerializeField] private Button russianButton;
    [SerializeField] private Button englishButton;
    [SerializeField] private Button kyrgyzButton;

    [Header("Language Title")]
    [SerializeField] private TMP_Text languageTitleText;

    private int currentPage = 0;

    private void Start()
    {
        if (SaveManager.Instance.Data.introCompleted)
        {
            introRoot.SetActive(false);
            mainMenuRoot.SetActive(true);
            return;
        }

        mainMenuRoot.SetActive(false);

        SetupLanguageButtons();

        nextButton.onClick.AddListener(NextPage);

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;

        RefreshLocalization();

        ShowLanguagePage();
    }

    private void StartGame()
    {
        SaveManager.Instance.Data.introCompleted = true;

        SaveManager.Instance.Save();

        HideAllPages();

        introRoot.SetActive(false);

        mainMenuRoot.SetActive(true);
    }
    
    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    private void SetupLanguageButtons()
    {
        russianButton.onClick.AddListener(() =>
        {
            SelectLanguage(Language.Russian);
        });

        englishButton.onClick.AddListener(() =>
        {
            SelectLanguage(Language.English);
        });

        kyrgyzButton.onClick.AddListener(() =>
        {
            SelectLanguage(Language.Kyrgyz);
        });
    }

    private void SelectLanguage(Language language)
    {
        LocalizationManager.Instance.SetLanguage(language);

        currentPage = 0;

        ShowPage(currentPage);
    }

    private void OnLanguageChanged(Language language)
    {
        RefreshLocalization();
    }

    private void RefreshLocalization()
    {
        if (LocalizationManager.Instance == null)
            return;

        languageTitleText.text =
            TextFormatter.Format(
                L("language_select_title")
            );

        nextButtonText.text =
            TextFormatter.Format(
                L("next")
            );

        startButtonText.text =
            TextFormatter.Format(
                L("start_game")
            );

        foreach (var page in introPages)
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

    private void ShowLanguagePage()
    {
        languagePage.SetActive(true);

        foreach (var page in introPages)
        {
            page.pageObject.SetActive(false);
        }

        nextButton.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
    }

    private void ShowPage(int index)
    {
        if (index < 0 || index >= introPages.Count)
            return;

        HideAllPages();

        introPages[index].pageObject.SetActive(true);

        bool isLastPage = index >= introPages.Count - 1;

        nextButton.gameObject.SetActive(!isLastPage);
        startButton.gameObject.SetActive(isLastPage);
    }

    private void HideAllPages()
    {
        languagePage.SetActive(false);

        foreach (var page in introPages)
        {
            page.pageObject.SetActive(false);
        }
    }

    public void NextPage()
    {
        if (currentPage >= introPages.Count - 1)
            return;

        currentPage++;

        ShowPage(currentPage);
    }

    private string L(string key)
    {
        return LocalizationManager.Instance
            .GetText("MainMenu", key);
    }
}