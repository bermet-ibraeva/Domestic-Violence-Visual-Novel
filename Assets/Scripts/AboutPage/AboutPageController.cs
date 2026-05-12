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
    [SerializeField] private DocumentPopupController documentPopup;

    private void Start()
    {
        if (feedbackButton != null)
        {
            feedbackButton.onClick.AddListener(OpenFeedbackPopup);
        }

        if (feedbackPopup != null)
        {
            feedbackPopup.SetActive(false);
        }

        if (termsButton != null)
        {
            termsButton.onClick.AddListener(OpenTermsPopup);
        }

        if (privacyButton != null)
        {
            privacyButton.onClick.AddListener(OpenPrivacyPopup);
        }

    }

    private void OnEnable()
    {
        RefreshLocalization();

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

        if (feedbackButton != null)
        {
            feedbackButton.onClick.RemoveListener(OpenFeedbackPopup);
        }

        if (termsButton != null)
        {
            termsButton.onClick.RemoveListener(OpenTermsPopup);
        }

        if (privacyButton != null)
        {
            privacyButton.onClick.RemoveListener(OpenPrivacyPopup);
        }

    }

    private void RefreshLocalization()
    {
        if (feedbackButtonText != null)
        {
            feedbackButtonText.text = L("leave_feedback");
        }

        if (termsButtonText != null)
        {
            termsButtonText.text = L("terms");
        }

        if (privacyButtonText != null)
        {
            privacyButtonText.text = L("privacy");
        }
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

    public void OpenTermsPopup()
    {
        if (documentPopup != null)
        {
            documentPopup.OpenPopup(
                L("terms"),
                L("terms_content"),
                L("read"));
        }
    }

    public void OpenPrivacyPopup()
    {
        if (documentPopup != null)
        {
            documentPopup.OpenPopup(
                L("privacy"),
                L("privacy_content"),
                L("read"));
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