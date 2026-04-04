using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
BackgroundController

This class manages background images and visual effects for a visual novel.

Responsibilities:
- Stores a database of background sprites (key -> Sprite)
- Provides a main Image component to display backgrounds
- Automatically builds a dictionary from the list of BackgroundEntry for fast lookup
- Fits backgrounds to screen while preserving aspect ratio
- Supports changing backgrounds with optional preset effects
- Resets background transforms to default when applying a new background
- Supports animated background effects via Coroutines:
    • zoom_in
    • pan_left / pan_right
    • tilt
    • shake
    • combined effects like zoom_in_pan_left / zoom_in_pan_right
    • drift_left / drift_right
- Allows stopping ongoing effects immediately
- Handles root transform (for pan/rotate/shake) and image transform (for zoom)
- Warns if a requested background or preset is missing
- Smoothly interpolates effects over time using Lerp
- Designed to integrate with dialogue system (e.g., called from DialogueController)

Usage:
- Call ApplyBackground("key", "preset") to set a background and optionally start an effect
- Call StopEffect() to halt ongoing animations
- Call ResetTransform() to return background to default state
*/

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

        if (backgroundImage != null)
        {
            if (imageTransform == null)
                imageTransform = backgroundImage.rectTransform;

            if (rootTransform == null)
                rootTransform = backgroundImage.rectTransform.parent as RectTransform;
        }
    }

    // --------------------------
    // APPLY BACKGROUND
    // --------------------------
    public void ApplyBackground(string key, string preset = null)
    {
        if (!bgDict.TryGetValue(key, out var sprite))
        {
            Debug.LogWarning($"Background '{key}' not found.");
            return;
        }

        // Остановить эффект
        StopEffect();

        // Сбросить трансформ (новый фон = чистое состояние)
        ResetTransform();

        // Поставить новый спрайт
        backgroundImage.sprite = sprite;
        backgroundImage.SetNativeSize();
        FitBackgroundToScreen();

        // Запустить эффект если есть
        if (!string.IsNullOrEmpty(preset) && preset != "none")
            PlayEffect(preset);
    }

    private void FitBackgroundToScreen()
    {
        if (backgroundImage == null || backgroundImage.sprite == null)
            return;

        float spriteHeight = backgroundImage.sprite.rect.height;
        float screenHeight = ((RectTransform)backgroundImage.canvas.transform).rect.height;

        float scale = screenHeight / spriteHeight;
        backgroundImage.rectTransform.localScale = Vector3.one * scale;
        backgroundImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    // --------------------------
    // EFFECTS
    // --------------------------
    public void PlayEffect(string preset)
    {
        StopEffect();
        currentEffect = StartCoroutine(EffectRoutine(preset));
    }

    public void StopEffect()
    {
        if (currentEffect != null)
            StopCoroutine(currentEffect);

        currentEffect = null;
    }

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

    private IEnumerator EffectRoutine(string preset)
    {
        switch (preset)
        {
            case "zoom_in":
                yield return ZoomTo(1.18f, 1.2f);
                break;

            case "pan_left":
                yield return PanTo(new Vector2(-400f, 0f), 1.2f);
                break;

            case "pan_right":
                yield return PanTo(new Vector2(400f, 0f), 1.2f);
                break;

            case "tilt":
                yield return RotateTo(3f, 0.8f);
                break;

            case "shake":
                yield return Shake(20f, 0.5f);
                break;

            case "zoom_in_pan_left":
                StartCoroutine(ZoomTo(1.18f, 1.2f));
                yield return PanTo(new Vector2(-400f, 0f), 1.2f);
                break;
            
            case "zoom_in_pan_right":
                StartCoroutine(ZoomTo(1.18f, 1.2f));
                yield return PanTo(new Vector2(400f, 0f), 1.2f);
                break;

            case "drift_right":
                yield return PanTo(new Vector2(600f, 0f), 1.2f);
                break;

            case "drift_left":
                yield return PanTo(new Vector2(-600f, 0f), 1.2f);
                break;
                
            default:
                Debug.LogWarning($"Unknown background preset: {preset}");
                break;
        }
    }

    private IEnumerator ZoomTo(float targetScale, float duration)
    {
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

