using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BackgroundEntry
{
    public string key;
    public Sprite sprite;
}

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
            {
                Debug.LogWarning("Invalid BackgroundEntry found. Skipping.");
                continue;
            }

            if (bgDict.ContainsKey(b.key))
            {
                Debug.LogWarning($"Duplicate key '{b.key}' found. Skipping.");
                continue;
            }

            bgDict.Add(b.key, b.sprite);
        }

        if (backgroundImage == null)
        {
            Debug.LogError("Background Image is not assigned in the Inspector.");
        }

        if (backgroundImage != null)
        {
            if (imageTransform == null)
            {
                imageTransform = backgroundImage.rectTransform;
            }

            if (rootTransform == null)
            {
                rootTransform = backgroundImage.rectTransform.parent as RectTransform;
            }
        }
    }

    // --------------------------
    // Existing: instant set
    // --------------------------
    public void ApplyBackground(string key, bool fade, float fadeDuration, string preset)
    {
        if (fade)
        {
            SetBackgroundWithFadeAndEffect(key, fadeDuration, preset);
            return;
        }

        // ---- INSTANT VERSION ----

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
        }

        // Останавливаем текущий эффект, НО без reset
        StopEffect();

        // Меняем фон
        SetBackground(key);

        // После смены спрайта делаем reset (важно!)
        ResetTransform();

        // Запускаем эффект если есть
        if (!string.IsNullOrEmpty(preset) && preset != "none")
        {
            PlayEffect(preset);
        }
    }
    // ------------------------------------------------------------------
    // NEW: плавная смена фона (fade out -> swap -> fade in)
    // ------------------------------------------------------------------
    private void SetBackground(string key)
    {
        if (backgroundImage == null)
            return;

        if (string.IsNullOrEmpty(key))
            return;

        if (bgDict.TryGetValue(key, out var sprite))
        {
            backgroundImage.sprite = sprite;
            backgroundImage.SetNativeSize();
            FitBackgroundToScreen();
            SetAlpha(1f);
        }
        else
        {
            Debug.LogWarning($"Background '{key}' not found.");
        }
    }

    public void SetBackgroundWithFadeAndEffect(string key, float duration, string preset)
    {
        if (backgroundImage == null)
            return;

        if (!bgDict.TryGetValue(key, out var sprite))
            return;

        if (duration <= 0f)
            duration = defaultFadeDuration;

        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(
            FadeSwapAndEffectRoutine(sprite, duration, preset)
        );
    }

    private void FitBackgroundToScreen()
    {
        if (backgroundImage == null)
            return;

        if (backgroundImage.sprite == null)
            return;

        RectTransform rt = backgroundImage.rectTransform;

        float spriteWidth = backgroundImage.sprite.rect.width;
        float spriteHeight = backgroundImage.sprite.rect.height;

        float screenHeight = ((RectTransform)backgroundImage.canvas.transform).rect.height;

        // Масштабируем так, чтобы высота = высоте экрана
        float scale = screenHeight / spriteHeight;

        rt.localScale = Vector3.one * scale;

        // Центрируем
        rt.anchoredPosition = Vector2.zero;
    }


    // ------------------------------------------------------------------
    // NEW: универсальный fade alpha
    // ------------------------------------------------------------------
    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (backgroundImage == null)
            yield break;

        float startAlpha = backgroundImage.color.a;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float progress = Mathf.Clamp01(t / duration);
            progress = 1 - Mathf.Pow(1 - progress, 3);

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private IEnumerator FadeSwapAndEffectRoutine(Sprite nextSprite, float duration, string preset)
    {
        if (backgroundImage.sprite == nextSprite)
        {
            currentTransition = null;

            if (!string.IsNullOrEmpty(preset) && preset != "none")
                PlayEffect(preset);

            yield break;
        }
        // Stop any active FX first
        StopEffect();

        // Fade out
        yield return FadeTo(0f, duration * 0.5f);

        ResetTransform();

        // Swap sprite
        backgroundImage.sprite = nextSprite;
        backgroundImage.SetNativeSize();
        FitBackgroundToScreen(); 
        // Fade in
        yield return FadeTo(1f, duration * 0.5f);

        currentTransition = null;

        if (!string.IsNullOrEmpty(preset) && preset != "none")
        {
            PlayEffect(preset);
        }
    }

    private void SetAlpha(float a)
    {
        if (backgroundImage == null)
        {
            return;
        }

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
        Debug.Log("PlayEffect called with: " + preset);

        if (string.IsNullOrEmpty(preset) || preset == "none")
        {
            StopEffect();
            return;
        }

        StopEffect();
        currentEffect = StartCoroutine(EffectRoutine(preset));
    }

    public void StopEffect()
    {
        if (currentEffect != null)
            StopCoroutine(currentEffect);

        currentEffect = null;
    }

    // Just effect without changing bg
    public void ApplyEffectOnly(string preset, bool stopPrevious = true)
    {
        if (string.IsNullOrEmpty(preset) || preset == "none")
            return;

        if (stopPrevious)
            StopEffect();

        PlayEffect(preset);
    }

    private IEnumerator EffectRoutine(string preset)
    {
        Debug.Log("EffectRoutine started: " + preset);

        switch (preset)
        {
            case "zoom_in":
                yield return ZoomTo(1.18f, 1.2f);
                break;
            case "zoom_out":
                yield return ZoomTo(1f, 1f);
                break;
            case "pan_left":
                yield return PanTo(new Vector2(-200f, 0f), 1.2f);
                break;
            case "pan_right":
                yield return PanTo(new Vector2(200f, 0f), 1.2f);;
                break;
            case "tilt":
                yield return RotateTo(3f, 0.8f);
                break;
            case "shake":
                yield return Shake(20f, 0.5f);
                break;
            case "zoom_in_pan_left":
                StartCoroutine(ZoomTo(1.18f, 1.2f));
                yield return PanTo(new Vector2(600f, 0f), 1.2f);
                break;
            case "drift_right":
                yield return PanTo(new Vector2(400f, 0f), 5f);
                break;
            case "drift_left":
                yield return PanTo(new Vector2(-400f, 0f), 5f);
                break;
            default:
                Debug.LogWarning($"Unknown background preset: {preset}");
                break;
        }
    }

    private IEnumerator ZoomTo(float targetScale, float duration, bool hold = true)
    {
        if (imageTransform == null)
        {
            yield break;
        }

        Vector3 start = imageTransform.localScale;
        Vector3 end = Vector3.one * targetScale;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            imageTransform.localScale = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }

        if (!hold)
        {
            imageTransform.localScale = end;
        }
    }

    private IEnumerator PanTo(Vector2 target, float duration)
    {
        if (rootTransform == null)
        {
            yield break;
        }

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
        if (rootTransform == null)
        {
            yield break;
        }

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
        if (rootTransform == null)
        {
            yield break;
        }

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
}