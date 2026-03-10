using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
CharacterPortraitController

This class manages character portraits and their emotions for a visual novel.

Responsibilities:
- Stores character data including default sprites and emotion-specific sprites
- Builds fast lookup dictionaries for characters and their emotions
- Provides a single Image component to display the portrait
- Supports showing a character with a specific emotion or default if emotion is missing
- Normalizes emotion names for consistent lookup
- Hides the portrait if no character or sprite is found
- Warns in the console if a sprite is missing or characterImage is not assigned
- Designed to integrate with DialogueController for dynamic dialogue display

Usage:
- Call Show(characterName, emotion) to display a character's portrait with the specified emotion
- Call Hide() to hide the portrait
- Supports multiple characters with multiple emotions each
- Automatically falls back to default sprite if the requested emotion sprite is not found
*/

public class CharacterPortraitController : MonoBehaviour
{
    public Image characterImage;

    [System.Serializable]
    public class EmotionSprite
    {
        public string emotionName;
        public Sprite sprite;
    }

    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public Sprite defaultSprite;
        public EmotionSprite[] emotions;
    }

    public List<CharacterData> characters = new();

    private Dictionary<string, Dictionary<string, Sprite>> emotionDict;
    private Dictionary<string, Sprite> defaultDict;

    void Awake()
    {
        if (characterImage == null)
            characterImage = GetComponentInChildren<Image>(true);

        BuildDictionaries();
    }

    void BuildDictionaries()
    {
        emotionDict = new();
        defaultDict = new();

        foreach (var ch in characters)
        {
            if (string.IsNullOrEmpty(ch.characterName))
                continue;

            defaultDict[ch.characterName] = ch.defaultSprite;

            var inner = new Dictionary<string, Sprite>();

            if (ch.emotions != null)
            {
                foreach (var em in ch.emotions)
                {
                    if (em == null || string.IsNullOrEmpty(em.emotionName) || em.sprite == null) continue;
                    inner[Normalize(em.emotionName)] = em.sprite;
                }
            }

            emotionDict[ch.characterName] = inner;
        }
    }

    static string Normalize(string s) => (s ?? "").Trim().ToLowerInvariant();

    public void Hide()
    {
        if (characterImage != null) characterImage.enabled = false;
    }

    public void Show(string characterName, string emotion)
    {
        if (characterImage == null)
        {
            Debug.LogWarning("[CharacterPortraitController] characterImage не назначен!");
            return;
        }

        if (string.IsNullOrEmpty(characterName))
        {
            Hide();
            return;
        }

        string emoKey = Normalize(emotion);
        Sprite result = null;

        if (!string.IsNullOrEmpty(emoKey) &&
            emotionDict != null &&
            emotionDict.TryGetValue(characterName, out var emDict) &&
            emDict != null &&
            emDict.TryGetValue(emoKey, out var emSprite))
        {
            result = emSprite;
        }

        if (result == null &&
            defaultDict != null &&
            defaultDict.TryGetValue(characterName, out var defSprite) &&
            defSprite != null)
        {
            result = defSprite;
        }

        if (result == null)
        {
            Debug.LogWarning($"[CharacterPortraitController] Нет спрайта для '{characterName}' (эмоция '{emotion}')");
            Hide();
            return;
        }

        characterImage.enabled = true;
        characterImage.sprite = result;
    }
}