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
    }

    public void ShowStatChange(string statKey, int value)
    {
        if (value == 0 || string.IsNullOrEmpty(statKey))
            return;

        StatPopupData data = BuildPopupData(statKey, value);

        // если statKey не должен показываться — просто не добавляем в очередь
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
        // RebuildCenteredLayout();

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

    private StatPopupData BuildPopupData(string statKey, int value)
    {
        string amountText = value > 0 ? $"+{value}" : value.ToString();

        // TODO: change to some kind of localization later
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

            // искорки не показываем в обычном игровом popup
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

    // // can be used for other codes too !!!!
    // private void RebuildCenteredLayout()
    // {
    //     if (labelText == null) return;

    //     RectTransform textRect = labelText.GetComponent<RectTransform>();
    //     RectTransform iconRect = iconImage != null ? iconImage.GetComponent<RectTransform>() : null;

    //     Canvas.ForceUpdateCanvases();
    //     labelText.ForceMeshUpdate();

    //     float textWidth = labelText.preferredWidth;
    //     bool hasIcon = iconImage != null && iconImage.enabled;

    //     float spacing = 10f;
    //     float iconWidth = 24f;

    //     float totalWidth = textWidth + (hasIcon ? iconWidth + spacing : 0f);

    //     float startX = -totalWidth * 0.5f;

    //     if (hasIcon && iconRect != null)
    //     {
    //         iconRect.anchoredPosition = new Vector2(startX + iconWidth * 0.5f, 0f);
    //     }

    //     float textX = hasIcon ? startX + iconWidth + spacing : startX;

    //     textRect.pivot = new Vector2(0f, 0.5f);
    //     textRect.anchoredPosition = new Vector2(textX, 0f);
    // }
}