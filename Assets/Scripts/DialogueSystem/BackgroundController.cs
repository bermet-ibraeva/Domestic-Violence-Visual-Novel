using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    [Header("Main Image")]
    [SerializeField] private Image backgroundImage;

    [Header("Background Database")]
    [SerializeField] private List<BackgroundEntry> backgrounds;

    private Dictionary<string, Sprite> bgDict;

    [Header("Animation Setup")]
    [SerializeField] private RectTransform rootTransform;   // pan/rotate/shake
    [SerializeField] private RectTransform imageTransform;  // zoom

    private Coroutine currentEffect;

    // --- NEW: Transition (fade) ---
    [Header("Transition")]
    [Tooltip("Default fade duration for background transitions.")]
    [SerializeField] private float defaultFadeDuration = 0.35f;

    private Coroutine currentTransition;

    private void Awake()
    {
        bgDict = new Dictionary<string, Sprite>();

        foreach (var b in backgrounds)
        {
            if (b == null || string.IsNullOrEmpty(b.key) || b.sprite == null)
                continue;

            if (!bgDict.ContainsKey(b.key))
                bgDict.Add(b.key, b.sprite);
        }

        if (backgroundImage != null && imageTransform == null)
            imageTransform = backgroundImage.rectTransform;

        if (backgroundImage != null && rootTransform == null)
            rootTransform = backgroundImage.rectTransform.parent as RectTransform;
    }

    // --------------------------
    // Existing: instant set
    // --------------------------
    public void SetBackground(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (bgDict.TryGetValue(key, out var sprite))
        {
            backgroundImage.sprite = sprite;
            ResetTransform();
            // ensure fully visible
            SetAlpha(1f);
        }
        else
        {
            Debug.LogWarning($"Background '{key}' not found in BackgroundController.");
        }
    }

    // ------------------------------------------------------------------
    // NEW: плавная смена фона (fade out -> swap -> fade in)
    // ------------------------------------------------------------------
    public void SetBackgroundWithFade(string key, float duration = -1f)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (!bgDict.TryGetValue(key, out var sprite))
        {
            Debug.Log($"[BG DICT COUNT] {bgDict.Count}");
            Debug.LogWarning($"Background '{key}' not found in BackgroundController.");
            return;
        }

        if (duration <= 0f)
            duration = defaultFadeDuration;

        // если уже идёт переход — остановим
        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(FadeSwapRoutine(sprite, duration));

        Debug.Log($"Current sprite: {backgroundImage.sprite?.name}");
        Debug.Log($"Next sprite: {sprite?.name}");
    }

    private IEnumerator FadeSwapRoutine(Sprite nextSprite, float duration)
    {
        // 1) остановить эффекты и сбросить трансформ перед переходом
        StopEffect(true);

        // 2) fade out
        yield return FadeTo(0f, duration * 0.5f);

        // 3) сменить картинку
        backgroundImage.sprite = nextSprite;

        // 4) reset transform (на всякий)
        ResetTransform();

        // 5) fade in
        yield return FadeTo(1f, duration * 0.5f);

        currentTransition = null;
    }

    // ------------------------------------------------------------------
    // NEW: универсальный fade alpha
    // ------------------------------------------------------------------
    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (backgroundImage == null) yield break;

        float startAlpha = backgroundImage.color.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float a)
    {
        if (backgroundImage == null) return;
        var c = backgroundImage.color;
        c.a = a;
        backgroundImage.color = c;
    }

    // --------------------------
    // Existing: reset transforms
    // --------------------------
    public void ResetTransform()
    {
        if (rootTransform != null)
        {
            rootTransform.anchoredPosition = Vector2.zero;
            rootTransform.localRotation = Quaternion.identity;
        }

        if (imageTransform != null)
        {
            imageTransform.localScale = Vector3.one;
        }
    }

    // --------------------------
    // Existing: presets
    // --------------------------
    public void PlayEffect(string preset)
    {
        if (string.IsNullOrEmpty(preset) || preset == "none")
        {
            StopEffect(true);
            return;
        }

        StopEffect(true);
        currentEffect = StartCoroutine(EffectRoutine(preset));
    }

    public void StopEffect(bool reset = false)
    {
        if (currentEffect != null)
            StopCoroutine(currentEffect);

        currentEffect = null;

        if (reset)
            ResetTransform();
    }

    private IEnumerator EffectRoutine(string preset)
    {
        switch (preset)
        {
            case "zoom_in":
                yield return ZoomTo(1.08f, 1f);
                break;

            case "zoom_out":
                yield return ZoomTo(1f, 1f);
                break;

            case "pan_left":
                yield return PanTo(new Vector2(-100f, 0f), 1.2f);
                break;

            case "pan_right":
                yield return PanTo(new Vector2(100f, 0f), 1.2f);
                break;

            case "tilt":
                yield return RotateTo(3f, 0.8f);
                break;

            case "shake":
                yield return Shake(20f, 0.5f);
                break;

            case "zoom_in_pan_right":
                yield return RunParallel(
                    ZoomTo(1.1f, 1.2f),
                    PanTo(new Vector2(120f, 0f), 1.2f)
                );
                break;

            default:
                Debug.LogWarning($"Unknown background preset: {preset}");
                break;
        }
    }

    private IEnumerator ZoomTo(float targetScale, float duration)
    {
        if (imageTransform == null) yield break;

        Vector3 start = imageTransform.localScale;
        Vector3 end = Vector3.one * targetScale;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            imageTransform.localScale = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }

        imageTransform.localScale = end;
    }

    private IEnumerator PanTo(Vector2 target, float duration)
    {
        if (rootTransform == null) yield break;

        Vector2 start = rootTransform.anchoredPosition;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            rootTransform.anchoredPosition = Vector2.Lerp(start, target, t / duration);
            yield return null;
        }

        rootTransform.anchoredPosition = target;
    }

    private IEnumerator RotateTo(float angle, float duration)
    {
        if (rootTransform == null) yield break;

        float start = rootTransform.localRotation.eulerAngles.z;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float z = Mathf.LerpAngle(start, angle, t / duration);
            rootTransform.localRotation = Quaternion.Euler(0, 0, z);
            yield return null;
        }

        rootTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    private IEnumerator Shake(float intensity, float duration)
    {
        if (rootTransform == null) yield break;

        Vector2 originalPos = rootTransform.anchoredPosition;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity, intensity);
            rootTransform.anchoredPosition = originalPos + new Vector2(x, y);
            yield return null;
        }

        rootTransform.anchoredPosition = originalPos;
    }

    // NOTE: сейчас параллель не идеальный (последовательный), но ок для старта.
    // Если хочешь реально параллельно — сделаем Wrap-функцию (как раньше).
    private IEnumerator RunParallel(IEnumerator a, IEnumerator b)
    {
        // Быстрый вариант: запускаем второй, ждём первый, потом ждём второй (почти параллельно)
        Coroutine cb = StartCoroutine(b);
        yield return StartCoroutine(a);
        yield return cb;
    }
}

[System.Serializable]
public class BackgroundEntry
{
    public string key;
    public Sprite sprite;
}