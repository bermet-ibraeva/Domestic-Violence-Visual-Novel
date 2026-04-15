using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SparkRewardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text rewardText;

    [Header("Optional Layout Root")]
    [SerializeField] private RectTransform panelRect;

    [Header("Icon")]
    [SerializeField] private Sprite sparksIcon;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float visibleDuration = 1.2f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    [Header("Motion")]
    [SerializeField] private float startYOffset = 20f;
    [SerializeField] private float endYOffset = 0f;
    [SerializeField] private float fadeOutYOffset = 10f;

    private readonly Queue<int> queue = new Queue<int>();

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine playRoutine;
    private Vector2 baseAnchoredPos;
    private Action onFinished;

    public bool IsShowing => playRoutine != null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (rectTransform != null)
            baseAnchoredPos = rectTransform.anchoredPosition;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (iconImage != null)
        {
            iconImage.sprite = sparksIcon;
            iconImage.enabled = sparksIcon != null;
        }
    }

    public void ShowReward(int amount)
    {
        if (amount <= 0)
            return;

        queue.Enqueue(amount);

        if (playRoutine == null)
            playRoutine = StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        while (queue.Count > 0)
        {
            yield return ShowBanner(queue.Dequeue());
        }

        playRoutine = null;

        var callback = onFinished;
        onFinished = null;
        callback?.Invoke();
    }

    private IEnumerator ShowBanner(int amount)
    {
        if (rewardText == null || canvasGroup == null || rectTransform == null)
            yield break;

        rewardText.text = $"+{amount} Искорки";

        RefreshLayout();

        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = baseAnchoredPos + new Vector2(0f, startYOffset);

        float time = 0f;

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeInDuration);
            float eased = EaseOutCubic(t);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);
            rectTransform.anchoredPosition = Vector2.Lerp(
                baseAnchoredPos + new Vector2(0f, startYOffset),
                baseAnchoredPos + new Vector2(0f, endYOffset),
                eased
            );

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = baseAnchoredPos + new Vector2(0f, endYOffset);

        yield return new WaitForSeconds(visibleDuration);

        time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeOutDuration);
            float eased = EaseOutCubic(t);

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);
            rectTransform.anchoredPosition = Vector2.Lerp(
                baseAnchoredPos + new Vector2(0f, endYOffset),
                baseAnchoredPos + new Vector2(0f, endYOffset + fadeOutYOffset),
                eased
            );

            yield return null;
        }

        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = baseAnchoredPos;
    }

    private void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();

        if (rewardText != null)
            rewardText.ForceMeshUpdate();

        if (panelRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        if (rectTransform != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public void WaitUntilFinished(Action callback)
    {
        if (!IsShowing)
        {
            callback?.Invoke();
            return;
        }

        onFinished += callback;
    }
}