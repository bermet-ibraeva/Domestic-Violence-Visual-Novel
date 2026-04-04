using TMPro;
using UnityEngine;

public class AboutPageLocalizedUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text leaveFeedbackButtonText;
    [SerializeField] private TMP_Text termsButtonText;
    [SerializeField] private TMP_Text privacyButtonText;

    private void Start()
    {
        RefreshUI();

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(Language language)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (LocalizationManager.Instance == null)
            return;

        titleText.text = LocalizationManager.Instance.GetText("AboutPage", "title");
        descriptionText.text = LocalizationManager.Instance.GetText("AboutPage", "description");
        feedbackText.text = LocalizationManager.Instance.GetText("AboutPage", "feedback_text");
        leaveFeedbackButtonText.text = LocalizationManager.Instance.GetText("AboutPage", "leave_feedback");
        termsButtonText.text = LocalizationManager.Instance.GetText("AboutPage", "terms");
        privacyButtonText.text = LocalizationManager.Instance.GetText("AboutPage", "privacy");
    }
}