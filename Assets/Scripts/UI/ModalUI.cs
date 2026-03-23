using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModalUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Root")]
    public GameObject modalRoot;
    public CanvasGroup modalCanvasGroup;

    [Header("Content")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;

    [Header("Animation")]
    public float fadeDuration = 0.2f;

    private Action onClose;
    private Coroutine activeRoutine;
    private bool isVisible;

    private void Awake()
    {
        HideImmediate();
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

        if (modalRoot != null)
            modalRoot.SetActive(true);

        if (titleText != null)
            titleText.text = data.title ?? "";

        if (messageText != null)
            messageText.text = data.message ?? "";

        onClose = callback;
        isVisible = true;

        if (modalCanvasGroup != null)
        {
            modalCanvasGroup.alpha = 0f;
            modalCanvasGroup.interactable = true;
            modalCanvasGroup.blocksRaycasts = true;
        }

        activeRoutine = StartCoroutine(FadeInRoutine());
    }

    public void OnPointerClick(PointerEventData eventData)
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

        if (modalRoot != null)
            modalRoot.SetActive(false);

        Action callback = onClose;
        onClose = null;
        activeRoutine = null;
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

        if (modalRoot != null)
            modalRoot.SetActive(false);

        isVisible = false;
        onClose = null;
    }
}