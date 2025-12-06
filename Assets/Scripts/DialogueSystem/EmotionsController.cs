using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EmotionsController : MonoBehaviour
{
    [Header("UI Image персонажа")]
    public Image characterImage;

    [Header("Список эмоций персонажа (emotionName -> sprite)")]
    public List<EmotionSprite> emotions = new List<EmotionSprite>();

    private Dictionary<string, Sprite> emotionDict;
    private Sprite fallbackCalmSprite;   // запасной Calm

    void Awake()
    {
        emotionDict = new Dictionary<string, Sprite>();

        foreach (var e in emotions)
        {
            if (!emotionDict.ContainsKey(e.emotionName))
                emotionDict.Add(e.emotionName, e.sprite);

            if (e.emotionName == "Calm")
                fallbackCalmSprite = e.sprite;
        }

        if (fallbackCalmSprite == null)
        {
            Debug.LogWarning($"[EmotionsController] У персонажа '{gameObject.name}' нет эмоции Calm! " +
                             $"Добавь Calm в список, иначе fallback будет пустой.");
        }
    }

    /// <summary>
    /// Устанавливает эмоцию персонажа по имени. Если эмоции нет – ставит Calm.
    /// </summary>
    public void SetEmotion(string emotion)
    {
        if (characterImage == null)
        {
            Debug.LogError("[EmotionsController] characterImage = NULL, прикрепи Image!");
            return;
        }

        if (string.IsNullOrEmpty(emotion))
        {
            ApplySprite(fallbackCalmSprite);
            return;
        }

        if (emotionDict.TryGetValue(emotion, out Sprite sprite))
        {
            ApplySprite(sprite);
        }
        else
        {
            Debug.LogWarning($"[EmotionsController] Эмоция '{emotion}' не найдена у '{gameObject.name}'. → Calm");

            ApplySprite(fallbackCalmSprite);
        }
    }

    /// <summary>
    /// Устанавливает спрайт моментально (готово к будущему fade-эффекту).
    /// </summary>
    private void ApplySprite(Sprite sprite)
    {
        if (sprite == null) return;
        characterImage.sprite = sprite;
    }
}

[System.Serializable]
public class EmotionSprite
{
    public string emotionName; // например: "Calm", "Sad", "Scared", "Happy"
    public Sprite sprite;
}
