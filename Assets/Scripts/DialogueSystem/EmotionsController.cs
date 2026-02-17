public class EmotionsController : MonoBehaviour
{
    [Header("UI Image персонажа")]
    [SerializeField] private Image characterImage;

    [Header("Список эмоций (emotionName -> sprite)")]
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
        // Чтобы в редакторе словарь обновлялся при правках списка
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

            var key = Normalize(e.emotionName);
            if (string.IsNullOrEmpty(key)) continue;

            if (firstValid == null) firstValid = e.sprite;

            if (!emotionDict.ContainsKey(key))
                emotionDict.Add(key, e.sprite);
            else
                Debug.LogWarning($"[EmotionsController] Duplicate emotion '{key}' on '{gameObject.name}'. Using the first one.");

            if (fallbackSprite == null && Cmp.Equals(key, Normalize(fallbackEmotion)))
                fallbackSprite = e.sprite;
        }

        // Если Calm (или fallbackEmotion) не найден — берём первый валидный спрайт как запасной
        if (fallbackSprite == null)
            fallbackSprite = firstValid;

        if (fallbackSprite == null)
        {
            Debug.LogWarning($"[EmotionsController] No valid emotion sprites on '{gameObject.name}'.");
        }
    }

    /// <summary>
    /// Set emotion by name. If not found -> fallback.
    /// </summary>
    public void SetEmotion(string emotion)
    {
        if (characterImage == null)
        {
            Debug.LogError($"[EmotionsController] characterImage is NULL on '{gameObject.name}'. Assign Image!");
            return;
        }

        // На случай если вызвали SetEmotion до Awake()
        if (emotionDict == null || emotionDict.Count == 0)
            BuildDictionary();

        var key = Normalize(emotion);

        if (!string.IsNullOrEmpty(key) && emotionDict.TryGetValue(key, out var sprite) && sprite != null)
        {
            ApplySprite(sprite);
            return;
        }

        // fallback
        if (fallbackSprite != null)
        {
            if (!string.IsNullOrEmpty(key))
                Debug.LogWarning($"[EmotionsController] Emotion '{emotion}' not found on '{gameObject.name}'. -> {fallbackEmotion}");

            ApplySprite(fallbackSprite);
        }
        else
        {
            Debug.LogWarning($"[EmotionsController] Fallback sprite is NULL on '{gameObject.name}'. Nothing to apply.");
        }
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

[Serializable]
public class EmotionSprite
{
    public string emotionName; // "Calm", "Happy", "Sad", ...
    public Sprite sprite;
}
