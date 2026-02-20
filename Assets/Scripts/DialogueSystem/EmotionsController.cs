using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmotionsController : MonoBehaviour
{
    [Header("UI Image персонажа")]
    [SerializeField] private Image characterImage;

    [Header("Список эмоций (emotion -> sprite)")]
    [SerializeField] private List<EmotionSprite> emotions = new List<EmotionSprite>();

    [Header("Fallback emotion key")]
    [SerializeField] private string fallbackEmotion = "Calm";

    private Dictionary<string, Sprite> emotionDict;
    private Sprite fallbackSprite;

    private static readonly StringComparer Cmp = StringComparer.OrdinalIgnoreCase;

    private void Awake()
    {
        BuildDictionary();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildDictionary();
    }
#endif

    private void BuildDictionary()
    {
        if (emotionDict == null)
            emotionDict = new Dictionary<string, Sprite>(Cmp);
        else
            emotionDict.Clear();

        fallbackSprite = null;
        Sprite firstValid = null;

        foreach (var e in emotions)
        {
            if (e == null || e.sprite == null) continue;

            // ✅ тут главное исправление
            var key = Normalize(e.emotion);
            if (string.IsNullOrEmpty(key)) continue;

            if (firstValid == null) firstValid = e.sprite;

            if (!emotionDict.ContainsKey(key))
                emotionDict.Add(key, e.sprite);
            else
                Debug.LogWarning($"[EmotionsController] Duplicate emotion '{key}' on '{gameObject.name}'. Using the first one.");

            if (fallbackSprite == null && Cmp.Equals(key, Normalize(fallbackEmotion)))
                fallbackSprite = e.sprite;
        }

        if (fallbackSprite == null)
            fallbackSprite = firstValid;

        if (fallbackSprite == null)
            Debug.LogWarning($"[EmotionsController] No valid emotion sprites on '{gameObject.name}'.");
    }

    public void SetEmotion(string emotion)
{
    if (characterImage == null)
    {
        Debug.LogError($"[EmotionsController] characterImage is NULL on '{gameObject.name}'. Assign Image!");
        return;
    }

    if (emotionDict == null || emotionDict.Count == 0)
        BuildDictionary();

    string key = Normalize(string.IsNullOrWhiteSpace(emotion) ? fallbackEmotion : emotion);

    if (!string.IsNullOrEmpty(key) &&
        emotionDict.TryGetValue(key, out var sprite) &&
        sprite != null)
    {
        ApplySprite(sprite);
        return;
    }

    // если конкретная эмоция не найдена — пробуем fallback
    if (!Cmp.Equals(key, Normalize(fallbackEmotion)) &&
        emotionDict.TryGetValue(Normalize(fallbackEmotion), out var fallback) &&
        fallback != null)
    {
        Debug.LogWarning($"[EmotionsController] Emotion '{emotion}' not found on '{gameObject.name}'. Using fallback '{fallbackEmotion}'.");
        ApplySprite(fallback);
        return;
    }

    Debug.LogWarning($"[EmotionsController] No valid emotion sprite on '{gameObject.name}'.");
}

    private static string Normalize(string s)
    {
        return string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim();
    }

    private void ApplySprite(Sprite sprite)
    {
        if (sprite == null) return;
        characterImage.sprite = sprite;
    }
}
