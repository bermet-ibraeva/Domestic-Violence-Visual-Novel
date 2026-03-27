using System;
using UnityEngine;

public class NotificationController : MonoBehaviour
{
    [Header("UI")]
    public ModalUI modalUI;
    public ToastUI toastUI;

    private Action onModalClosed;
    private bool isModalShowing;

    public bool IsModalShowing => isModalShowing;

    private void Awake()
    {
        HideAllImmediate();
    }

    public void Show(NotificationData data, Action onComplete = null)
    {
        if (data == null)
        {
            onComplete?.Invoke();
            return;
        }

        string mode = string.IsNullOrWhiteSpace(data.mode)
            ? ""
            : data.mode.Trim().ToLower();

        switch (mode)
        {
            case "modal":
                ShowModal(data, onComplete);
                break;

            case "toast":
                ShowToast(data);
                onComplete?.Invoke();
                break;

            default:
                onComplete?.Invoke();
                break;
        }
    }

    private void ShowModal(NotificationData data, Action onComplete = null)
    {
        isModalShowing = true;
        onModalClosed = onComplete;

        if (modalUI != null)
        {
            modalUI.Show(data, () =>
            {
                isModalShowing = false;

                Action callback = onModalClosed;
                onModalClosed = null;
                callback?.Invoke();
            });
        }
        else
        {
            isModalShowing = false;
            onModalClosed = null;
            onComplete?.Invoke();
        }
    }

    private void ShowToast(NotificationData data)
    {
        if (toastUI != null)
            toastUI.Show(data);
    }

    public void HideAllImmediate()
    {
        if (modalUI != null)
            modalUI.HideImmediate();

        if (toastUI != null)
            toastUI.HideImmediate();

        isModalShowing = false;
        onModalClosed = null;
    }
}