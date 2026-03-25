using System;
using UnityEngine;

public class NotificationController : MonoBehaviour
{
    [Header("UI")]
    public ModalUI modalUI;
    public ToastUI toastUI;

    private Action onClosed;
    private bool isBlockingShown;

    public bool IsShowingBlockingNotification => isBlockingShown;

    void Awake()
    {
        if (modalUI != null)
            modalUI.HideImmediate();

        if (toastUI != null)
            toastUI.HideImmediate();
    }

    public bool IsBlocking(NotificationData data)
    {
        if (data == null || string.IsNullOrEmpty(data.mode))
            return false;

        return data.mode.ToLower() == "modal";
    }

    public void Show(NotificationData data, Action onComplete = null)
    {
        if (data == null)
        {
            onComplete?.Invoke();
            return;
        }

        string mode = string.IsNullOrEmpty(data.mode) ? "" : data.mode.ToLower();

        if (mode == "modal")
        {
            isBlockingShown = true;
            onClosed = onComplete;

            if (modalUI != null)
            {
                modalUI.Show(data, () =>
                {
                    isBlockingShown = false;

                    Action callback = onClosed;
                    onClosed = null;
                    callback?.Invoke();
                });
            }
            else
            {
                isBlockingShown = false;
                onComplete?.Invoke();
            }

            return;
        }

        if (mode == "toast")
        {
            if (toastUI != null)
                toastUI.Show(data);

            onComplete?.Invoke();
            return;
        }

        onComplete?.Invoke();
    }

    public void HideAllImmediate()
    {
        if (modalUI != null)
            modalUI.HideImmediate();

        if (toastUI != null)
            toastUI.HideImmediate();

        isBlockingShown = false;
        onClosed = null;
    }
}