using System;
using UnityEngine;
using TMPro;

public class NotificationController : MonoBehaviour
{
    [Header("Roots")]
    public GameObject modalRoot;
    public GameObject toastRoot;

    [Header("Text")]
    public TextMeshProUGUI modalTitleText;
    public TextMeshProUGUI modalBodyText;
    public TextMeshProUGUI toastText;

    [Header("Resize")]
    public SimpleAutoResize modalResize;
    public SimpleAutoResize toastResize;

    private Action onClosed;
    private bool isBlockingShown = false;
    private bool ignoreFirstClick = false;

    public bool IsShowingBlockingNotification => isBlockingShown;

    void Awake()
    {
        HideAllImmediate();
    }

    void Update()
    {
        if (!isBlockingShown)
            return;

        if (ignoreFirstClick)
        {
            ignoreFirstClick = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            CloseModal();
        }
    }

    public bool IsBlocking(NotificationData data)
    {
        if (data == null || string.IsNullOrEmpty(data.type))
            return false;

        return data.type.ToLower() == "modal";
    }

    public void Show(NotificationData data, Action onComplete = null)
    {
        if (data == null)
            return;

        HideAllImmediate();

        onClosed = onComplete;

        string type = string.IsNullOrEmpty(data.type) ? "" : data.type.ToLower();

        if (type == "modal")
        {
            if (modalRoot != null)
                modalRoot.SetActive(true);

            if (modalTitleText != null)
                modalTitleText.text = data.title;

            if (modalBodyText != null)
            {
                modalBodyText.text = data.message;

                if (modalResize != null)
                    modalResize.SetText(data.message);
            }

            isBlockingShown = true;
            ignoreFirstClick = true;
            return;
        }

        if (type == "toast")
        {
            if (toastRoot != null)
                toastRoot.SetActive(true);

            if (toastText != null)
            {
                toastText.text = data.message;

                if (toastResize != null)
                    toastResize.SetText(data.message);
            }

            isBlockingShown = false;
        }
    }

    public void CloseModal()
    {
        if (modalRoot != null)
            modalRoot.SetActive(false);

        isBlockingShown = false;

        Action callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }

    public void HideAllImmediate()
    {
        if (modalRoot != null)
            modalRoot.SetActive(false);

        if (toastRoot != null)
            toastRoot.SetActive(false);

        isBlockingShown = false;
        onClosed = null;
        ignoreFirstClick = false;
    }
}