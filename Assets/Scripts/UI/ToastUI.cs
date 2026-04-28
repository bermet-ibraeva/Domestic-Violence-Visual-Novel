using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToastUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject toastRoot;
    public CanvasGroup toastCanvasGroup;

    [Header("Content")]
    public TextMeshProUGUI messageText;

    [Header("Timing")]
    public float visibleDuration = 2.2f;
    public float fadeDuration = 0.15f;

    private readonly Queue<NotificationData> queue = new Queue<NotificationData>();
    private Coroutine activeRoutine;
    private bool isShowing;

    private void Awake()
    {
        HideImmediate();
    }

    public void Show(NotificationData data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.messageKey))
            return;

        queue.Enqueue(data);

        if (!isShowing)
            activeRoutine = StartCoroutine(ProcessQueue());
    }

    public void HideImmediate()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        queue.Clear();
        isShowing = false;

        if (toastCanvasGroup != null)
        {
            toastCanvasGroup.alpha = 0f;
            toastCanvasGroup.interactable = false;
            toastCanvasGroup.blocksRaycasts = false;
        }

        if (toastRoot != null)
            toastRoot.SetActive(false);
    }

    private IEnumerator ProcessQueue()
    {
        isShowing = true;

        while (queue.Count > 0)
        {
            NotificationData data = queue.Dequeue();

            if (toastRoot != null)
                toastRoot.SetActive(true);

            if (toastCanvasGroup != null)
            {
                toastCanvasGroup.alpha = 0f;
                toastCanvasGroup.interactable = false;
                toastCanvasGroup.blocksRaycasts = false;
            }

            if (messageText != null)
                messageText.text = LocalizationManager.Instance.GetText("Notifications", data.messageKey);

            yield return FadeTo(1f);
            yield return new WaitForSeconds(visibleDuration);
            yield return FadeTo(0f);

            if (toastRoot != null)
                toastRoot.SetActive(false);
        }

        if (toastCanvasGroup != null)
        {
            toastCanvasGroup.alpha = 0f;
            toastCanvasGroup.interactable = false;
            toastCanvasGroup.blocksRaycasts = false;
        }

        if (toastRoot != null)
            toastRoot.SetActive(false);

        isShowing = false;
        activeRoutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (toastCanvasGroup == null)
            yield break;

        float startAlpha = toastCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = fadeDuration <= 0f ? 1f : time / fadeDuration;
            toastCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        toastCanvasGroup.alpha = targetAlpha;
    }
}