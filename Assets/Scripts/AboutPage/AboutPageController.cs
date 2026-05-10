using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AboutPageController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button feedbackButton;
    [SerializeField] private Button termsButton;
    [SerializeField] private Button privacyButton;

    [Header("Button Labels")]
    [SerializeField] private TMP_Text feedbackButtonText;
    [SerializeField] private TMP_Text termsButtonText;
    [SerializeField] private TMP_Text privacyButtonText;

    [Header("Popups")]
    [SerializeField] private GameObject feedbackPopup;

    private void Start()
    {
        RefreshLocalization();

        if (feedbackButton != null)
        {
            feedbackButton.onClick.AddListener(OpenFeedbackPopup);
        }

        if (feedbackPopup != null)
        {
            feedbackPopup.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += HandleLanguageChanged;
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
        }
    }

    private void RefreshLocalization()
    {
        feedbackButtonText.text = L("leave_feedback");

        termsButtonText.text = L("terms");

        privacyButtonText.text = L("privacy");
    }

    private string L(string key)
    {
        return LocalizationManager.Instance
            .GetText("AboutPage", key);
    }

    private void HandleLanguageChanged(Language language)
    {
        RefreshLocalization();
    }

    private void OpenFeedbackPopup()
    {
        if (feedbackPopup != null)
        {
            feedbackPopup.SetActive(true);
        }
    }

    public void CloseFeedbackPopup()
    {
        if (feedbackPopup != null)
        {
            feedbackPopup.SetActive(false);
        }
    }
}