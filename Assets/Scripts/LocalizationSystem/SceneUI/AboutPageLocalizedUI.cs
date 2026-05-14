using TMPro;
using UnityEngine;
using System.Collections;

public class AboutPageLocalizedUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text leaveFeedbackButtonText;
    [SerializeField] private TMP_Text termsButtonText;
    [SerializeField] private TMP_Text privacyButtonText;

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

    private void HandleLanguageChanged(Language language)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (LocalizationManager.Instance == null)
            return;

        if (titleText != null)
            titleText.text = L("title");

        if (descriptionText != null)
        {
            descriptionText.text =
                TextFormatter.Format(L("description"));
        }

        if (feedbackText != null)
            feedbackText.text = L("feedback_text");

        if (leaveFeedbackButtonText != null)
            leaveFeedbackButtonText.text = L("leave_feedback");

        if (termsButtonText != null)
            termsButtonText.text = L("terms");

        if (privacyButtonText != null)
            privacyButtonText.text = L("privacy");
    }

    private string L(string key)
    {
        return LocalizationManager.Instance.GetText("AboutPage", key);
    }
}