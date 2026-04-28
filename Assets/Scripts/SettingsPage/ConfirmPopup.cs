using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action onConfirm;

    private void Awake()
    {
        if (yesButton != null)
            yesButton.onClick.AddListener(OnYes);

        if (noButton != null)
            noButton.onClick.AddListener(Hide);

        Hide();
    }

    public void Show(string messageKey, Action confirmAction)
    {
        if (messageText != null)
            messageText.text = LocalizationManager.Instance.GetText("SettingsPage", messageKey);

        if (yesButton != null)
        {
            TMP_Text yesText = yesButton.GetComponentInChildren<TMP_Text>();
            if (yesText != null)
                yesText.text = LocalizationManager.Instance.GetText("SettingsPage", "yes");
        }

        if (noButton != null)
        {
            TMP_Text noText = noButton.GetComponentInChildren<TMP_Text>();
            if (noText != null)
                noText.text = LocalizationManager.Instance.GetText("SettingsPage", "no");
        }

        onConfirm = confirmAction;
        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);
        onConfirm = null;
    }

    private void OnYes()
    {
        onConfirm?.Invoke();
        Hide();
    }
}