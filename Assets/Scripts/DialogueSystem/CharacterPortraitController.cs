using System;
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

[Serializable]
public class EmotionSprite
{
    public string emotion;
    public Sprite sprite;
}

[Serializable]
public class CharacterData
{
    public string characterId;
    public Sprite defaultSprite;
    public List<EmotionSprite> emotions;
}


public class CharacterPortraitController : MonoBehaviour
{
    public Image characterImage;
    public List<CharacterData> characters = new();

    private Dictionary<string, Sprite> defaultDict;
    private Dictionary<string, Dictionary<string, Sprite>> emotionDict;

    void Awake()
    {
        if (characterImage == null)
            characterImage = GetComponentInChildren<Image>(true);

        BuildDictionaries();
    }

    void BuildDictionaries()
    {
        defaultDict = new();
        emotionDict = new();

        foreach (var ch in characters)
        {
            if (ch == null || string.IsNullOrEmpty(ch.characterId))
                continue;

            defaultDict[ch.characterId] = ch.defaultSprite;

            var inner = new Dictionary<string, Sprite>();

            if (ch.emotions != null)
            {
                foreach (var em in ch.emotions)
                {
                    if (em == null || string.IsNullOrEmpty(em.emotion) || em.sprite == null)
                        continue;

                    inner[Normalize(em.emotion)] = em.sprite;
                }
            }

            emotionDict[ch.characterId] = inner;
        }
    }

    static string Normalize(string s)
    {
        return (s ?? "").Trim().ToLowerInvariant();
    }

    public void Hide()
    {
        if (characterImage != null)
            characterImage.enabled = false;
    }

    public void Show(string characterId, string emotion)
    {
        if (characterImage == null)
        {
            Debug.LogWarning("[CharacterPortraitController] characterImage не назначен.");
            return;
        }

        if (string.IsNullOrEmpty(characterId))
        {
            Hide();
            return;
        }

        Sprite result = GetSprite(characterId, emotion);

        if (result == null)
        {
            Debug.LogWarning($"[CharacterPortraitController] Нет спрайта для '{characterId}' (эмоция '{emotion}')");
            Hide();
            return;
        }

        characterImage.enabled = true;
        characterImage.sprite = result;
    }

    public Sprite GetSprite(string characterId, string emotion)
    {
        if (string.IsNullOrEmpty(characterId))
            return null;

        string emoKey = Normalize(string.IsNullOrEmpty(emotion) ? "Calm" : emotion);

        if (emotionDict != null &&
            emotionDict.TryGetValue(characterId, out var emDict) &&
            emDict != null &&
            emDict.TryGetValue(emoKey, out var emSprite) &&
            emSprite != null)
        {
            return emSprite;
        }

        if (defaultDict != null &&
            defaultDict.TryGetValue(characterId, out var defSprite) &&
            defSprite != null)
        {
            return defSprite;
        }

        return null;
    }
}
