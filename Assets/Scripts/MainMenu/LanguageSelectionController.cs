using UnityEngine;


// This controller is responsible for handling the language selection during the first launch flow.
public class LanguageSelectionController : MonoBehaviour
{
    [SerializeField] private FirstLaunchFlowController firstLaunchFlowController;

    public void SelectRussian()
    {
        ApplyLanguage(Language.Russian);
    }

    public void SelectEnglish()
    {
        ApplyLanguage(Language.English);
    }

    public void SelectKyrgyz()
    {
        ApplyLanguage(Language.Kyrgyz);
    }

    private void ApplyLanguage(Language language)
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLanguage(language);

        if (firstLaunchFlowController != null)
            firstLaunchFlowController.OpenIntroStep();
    }
}