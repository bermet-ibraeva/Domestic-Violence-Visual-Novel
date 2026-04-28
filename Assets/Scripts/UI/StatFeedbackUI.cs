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

    public RectTransform statePanelRect;
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

    private readonly Queue<StatPopupData> queue = new();

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

    // ================= PUBLIC =================

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

    public void WaitUntilFinished(Action callback)
    {
        if (!IsShowing)
        {
            callback?.Invoke();
            return;
        }

        onFinished += callback;
    }

    // ================= CORE =================

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

        // icon
        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = data.icon != null;
        }

        // text
        labelText.text = data.text;

        // цвет (UX улучшение)
        labelText.color = data.value > 0 ? new Color(0.4f, 0.9f, 0.4f) : new Color(1f, 0.4f, 0.4f);

        RefreshFeedbackLayout();

        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = baseAnchoredPos + new Vector2(0f, startYOffset);

        float time = 0f;

        // fade in
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

        yield return new WaitForSeconds(visibleDuration);

        time = 0f;

        // fade out
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

    // ================= DATA =================

    private StatPopupData BuildPopupData(string statKey, int value)
    {
        // ❌ не показываем sparks
        if (statKey == "sparks")
            return new StatPopupData(null, null, value);

        string amountText = value > 0 ? $"+{value}" : value.ToString();

        string label = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.GetText("Stats", GetStatKey(statKey))
            : statKey;

        Sprite icon = GetIcon(statKey);

        return new StatPopupData($"{amountText} {label}", icon, value);
    }

    private string GetStatKey(string statKey)
    {
        switch (statKey)
        {
            case "trust.AG":
            case "trustAG": return "trustAG";

            case "trust.JA":
            case "trustJA": return "trustJA";

            case "risk": return "risk";
            case "safety": return "safety";

            default: return statKey;
        }
    }

    private Sprite GetIcon(string statKey)
    {
        switch (statKey)
        {
            case "trust.AG":
            case "trustAG": return trustAGIcon;

            case "trust.JA":
            case "trustJA": return trustJAIcon;

            case "risk": return riskIcon;
            case "safety": return safetyIcon;

            default: return null;
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
        public int value;

        public StatPopupData(string text, Sprite icon, int value)
        {
            this.text = text;
            this.icon = icon;
            this.value = value;
        }
    }
}