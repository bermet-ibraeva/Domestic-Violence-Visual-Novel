using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EmotionsController : MonoBehaviour
{
    [Header("UI Image персонажа")]
    public Image characterImage;

    [Header("Список эмоций персонажа")]
    public List<EmotionSprite> emotions = new List<EmotionSprite>();

    private Dictionary<string, Sprite> emotionDict;

    void Awake()
    {
        emotionDict = new Dictionary<string, Sprite>();

        foreach (var e in emotions)
        {
            if (!emotionDict.ContainsKey(e.emotionName))
                emotionDict.Add(e.emotionName, e.sprite);
        }
    }

    public void SetEmotion(string emotion)
    {
        if (characterImage == null) return;

        if (emotionDict.ContainsKey(emotion))
        {
            characterImage.sprite = emotionDict[emotion];
        }
        else
        {
            Debug.LogWarning($"Эмоция '{emotion}' не найдена у персонажа {gameObject.name}. Ставлю Calm.");

            if (emotionDict.ContainsKey("Calm"))
                characterImage.sprite = emotionDict["Calm"];
        }
    }
}

[System.Serializable]
public class EmotionSprite
{
    public string emotionName; // например: "Sad", "Warm", "Tense"
    public Sprite sprite;
}
