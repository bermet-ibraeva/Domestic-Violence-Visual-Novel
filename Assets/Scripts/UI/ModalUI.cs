using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ModalUI : MonoBehaviour
{
    [Header("Root")]
    public CanvasGroup modalCanvasGroup;

    [Header("Content")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;

    [Header("Animation")]
    public float fadeDuration = 0.2f;

    private Action onClose;
    private Coroutine activeRoutine;
    private bool isVisible;
    private int showFrame = -1;

    private void Awake()
    {
        HideImmediate();
    }

    private void Update()
    {
        if (!isVisible)
            return;

        // Игнорируем тот же кадр, в который modal был открыт
        if (Time.frameCount == showFrame)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            RequestClose();
        }
    }

    public void Show(NotificationData data, Action callback = null)
    {
        if (data == null)
        {
            callback?.Invoke();
            return;
        }

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (titleText != null)
            titleText.text = LocalizationManager.Instance.GetText("Episode" + data.titleKey) ?? "";

        if (messageText != null)
            messageText.text = LocalizationManager.Instance.GetText("Episode" + data.messageKey) ?? "";

        onClose = callback;
        isVisible = true;
        showFrame = Time.frameCount;

        if (modalCanvasGroup != null)
        {
            modalCanvasGroup.alpha = 0f;
            modalCanvasGroup.interactable = true;
            modalCanvasGroup.blocksRaycasts = true;
        }

        activeRoutine = StartCoroutine(FadeInRoutine());
    }

    public void RequestClose()
    {
        if (!isVisible)
            return;

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        activeRoutine = StartCoroutine(CloseRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        yield return FadeTo(1f);
        activeRoutine = null;
    }

    private IEnumerator CloseRoutine()
    {
        isVisible = false;

        if (modalCanvasGroup != null)
        {
            modalCanvasGroup.interactable = false;
            modalCanvasGroup.blocksRaycasts = false;
        }

        yield return FadeTo(0f);

        Action callback = onClose;
        onClose = null;
        activeRoutine = null;

        yield return null;
        callback?.Invoke();
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (modalCanvasGroup == null)
            yield break;

        float start = modalCanvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = fadeDuration <= 0f ? 1f : t / fadeDuration;
            modalCanvasGroup.alpha = Mathf.Lerp(start, targetAlpha, normalized);
            yield return null;
        }

        modalCanvasGroup.alpha = targetAlpha;
    }

    public void HideImmediate()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (modalCanvasGroup != null)
        {
            modalCanvasGroup.alpha = 0f;
            modalCanvasGroup.interactable = false;
            modalCanvasGroup.blocksRaycasts = false;
        }

        isVisible = false;
        onClose = null;
        showFrame = -1;
    }

    public bool IsVisible()
    {
        return isVisible;
    }
}