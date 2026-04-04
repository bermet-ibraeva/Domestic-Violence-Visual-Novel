using UnityEngine;

public class FirstLaunchFlowController : MonoBehaviour
{
    [Header("Root Objects")]
    [SerializeField] private GameObject firstLaunchRoot;
    [SerializeField] private GameObject mainMenuRoot;

    [Header("Steps")]
    [SerializeField] private GameObject languageStep;
    [SerializeField] private GameObject introStep;

    private void Start()
    {
        InitializeFlow();
    }

    private void InitializeFlow()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogWarning("[FirstLaunchFlow] LocalizationManager not found.");
            ShowMainMenu();
            return;
        }

        if (LocalizationManager.Instance.IsFirstLaunch())
        {
            ShowFirstLaunchFlow();
            OpenLanguageStep();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void ShowFirstLaunchFlow()
    {
        if (firstLaunchRoot != null)
            firstLaunchRoot.SetActive(true);

        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(false);
    }

    private void ShowMainMenu()
    {
        if (firstLaunchRoot != null)
            firstLaunchRoot.SetActive(false);

        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(true);
    }

    public void OpenLanguageStep()
    {
        if (languageStep != null)
            languageStep.SetActive(true);

        if (introStep != null)
            introStep.SetActive(false);
    }

    public void OpenIntroStep()
    {
        if (languageStep != null)
            languageStep.SetActive(false);

        if (introStep != null)
            introStep.SetActive(true);
    }

    public void FinishFirstLaunchFlow()
    {
        ShowMainMenu();
    }
}