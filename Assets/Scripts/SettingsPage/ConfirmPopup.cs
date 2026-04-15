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

    public void Show(string message, Action confirmAction)
    {
        if (messageText != null)
            messageText.text = message;

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