using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatFeedbackUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI labelText;

    [Tooltip("RectTransform объекта StatePanel, на котором висит Horizontal Layout Group + Content Size Fitter")]
    public RectTransform statePanelRect;

    [Tooltip("Скрипт TextMaxWidthClamp на LabelReward или LabelContainer")]
    public TextMaxWidthClamp labelWidthClamp;

    [Header("Icons")]
    public Sprite trustAGIcon;
    public Sprite trustJAIcon;
    public Sprite safetyIcon;
    public Sprite riskIcon;

    [Header("Timing")]
    public float fadeInDuration = 0.2f;
    public float visibleDuration = 1.8f;
    public float fadeOutDuration = 0.25f;

    [Header("Motion")]
    public float startYOffset = 18f;
    public float endYOffset = 0f;

    private readonly Queue<StatPopupData> queue = new Queue<StatPopupData>();

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

        if (labelWidthClamp == null && labelText != null)
            labelWidthClamp = labelText.GetComponent<TextMaxWidthClamp>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowStatChange(string statKey, int value)
    {
        if (value == 0 || string.IsNullOrEmpty(statKey))
            return;

        StatPopupData data = BuildPopupData(statKey, value);

        if (string.IsNullOrEmpty(data.text))
            return;

        queue.Enqueue(data);

        if (playRoutine == null)
            playRoutine = StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        while (queue.Count > 0)
            yield return ShowBanner(queue.Dequeue());

        playRoutine = null;

        var callback = onFinished;
        onFinished = null;
        callback?.Invoke();
    }

    private IEnumerator ShowBanner(StatPopupData data)
    {
        if (rectTransform == null || canvasGroup == null || labelText == null)
            yield break;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = data.icon != null;
        }

        labelText.text = data.text;

        // ВОТ ГЛАВНОЕ:
        // после смены текста обновляем ограничение ширины
        // и пересобираем layout, чтобы StatePanel остался по центру.
        RefreshFeedbackLayout();

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
                baseAnchoredPos + new Vector2(0f, endYOffset + 8f),
                eased
            );

            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        rectTransform.anchoredPosition = baseAnchoredPos;
    }

    private void RefreshFeedbackLayout()
    {
        Canvas.ForceUpdateCanvases();

        if (labelText != null)
            labelText.ForceMeshUpdate();

        if (labelWidthClamp != null)
            labelWidthClamp.RefreshWidth();

        if (statePanelRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(statePanelRect);

        if (rectTransform != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private StatPopupData BuildPopupData(string statKey, int value)
    {
        string amountText = value > 0 ? $"+{value}" : value.ToString();

        switch (statKey)
        {
            case "trust.AG":
                return new StatPopupData($"{amountText} Доверие А–Г", trustAGIcon);

            case "trust.JA":
                return new StatPopupData($"{amountText} Доверие Ж–А", trustJAIcon);

            case "safety":
                return new StatPopupData($"{amountText} Безопасность", safetyIcon);

            case "risk":
                return new StatPopupData($"{amountText} Риск", riskIcon);

            case "sparks":
                return new StatPopupData(null, null);

            default:
                return new StatPopupData($"{amountText} {statKey}", null);
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private struct StatPopupData
    {
        public string text;
        public Sprite icon;

        public StatPopupData(string text, Sprite icon)
        {
            this.text = text;
            this.icon = icon;
        }
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