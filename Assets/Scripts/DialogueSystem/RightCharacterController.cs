using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightCharacterController : MonoBehaviour
{
    [Header("UI Image правого персонажа")]
    public Image characterImage;

    [System.Serializable]
    public class EmotionSprite
    {
        public string emotionName;  // "Calm", "Sad", "Tense" и т.д.
        public Sprite sprite;
    }

    [System.Serializable]
    public class CharacterData
    {
        public string characterName;       // "Гулдана", "Учитель"
        public Sprite defaultSprite;       // базовый портрет
        public EmotionSprite[] emotions;   // доп. эмоции (можешь пока оставить пустым)
    }

    [Header("Персонажи справа")]
    public List<CharacterData> characters = new List<CharacterData>();

    // словарь: имя персонажа -> (эмоция -> спрайт)
    private Dictionary<string, Dictionary<string, Sprite>> emotionDict;
    // словарь: имя персонажа -> дефолтный спрайт
    private Dictionary<string, Sprite> defaultDict;

    void Awake()
    {
        BuildDictionaries();
    }

    void BuildDictionaries()
    {
        emotionDict = new Dictionary<string, Dictionary<string, Sprite>>();
        defaultDict = new Dictionary<string, Sprite>();

        foreach (var ch in characters)
        {
            if (string.IsNullOrEmpty(ch.characterName))
                continue;

            if (!defaultDict.ContainsKey(ch.characterName))
                defaultDict.Add(ch.characterName, ch.defaultSprite);

            var inner = new Dictionary<string, Sprite>();

            if (ch.emotions != null)
            {
                foreach (var em in ch.emotions)
                {
                    if (em == null || string.IsNullOrEmpty(em.emotionName) || em.sprite == null)
                        continue;

                    if (!inner.ContainsKey(em.emotionName))
                        inner.Add(em.emotionName, em.sprite);
                }
            }

            emotionDict[ch.characterName] = inner;
        }
    }

    /// <summary>
    /// Показать персонажа по имени и эмоции.
    /// Если эмоция не найдена — берём defaultSprite.
    /// Если и его нет — выводим предупреждение.
    /// </summary>
    public void Show(string characterName, string emotion)
    {
        if (characterImage == null)
        {
            Debug.LogWarning("[RightCharacterController] characterImage не назначен!");
            return;
        }

        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogWarning("[RightCharacterController] Пустое имя персонажа.");
            return;
        }

        Sprite result = null;

        // 1) Пытаемся найти эмоцию
        if (!string.IsNullOrEmpty(emotion) &&
            emotionDict != null &&
            emotionDict.TryGetValue(characterName, out var emDict) &&
            emDict != null &&
            emDict.TryGetValue(emotion, out var emSprite))
        {
            result = emSprite;
        }

        // 2) Если эмоция не найдена — пробуем дефолтный
        if (result == null &&
            defaultDict != null &&
            defaultDict.TryGetValue(characterName, out var defSprite) &&
            defSprite != null)
        {
            result = defSprite;
        }

        // 3) Если всё равно ничего — предупреждение
        if (result == null)
        {
            Debug.LogWarning($"[RightCharacterController] Нет спрайта для персонажа '{characterName}' (эмоция '{emotion}')");
            return;
        }

        characterImage.sprite = result;
    }
}
