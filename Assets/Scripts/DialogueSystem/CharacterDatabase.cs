using System.Collections.Generic;
using UnityEngine;

/*
CharacterDatabase

This class stores character sprites and emotion variants for a visual novel.

Responsibilities:
- Holds a list of CharacterDefinition objects, each with:
    • defaultSprite
    • optional emotion-specific sprites
- Builds fast lookup dictionaries for:
    • defaultDict: characterName -> default Sprite
    • emotionDict: characterName -> (emotionName -> Sprite)
- Provides GetSprite(character, emotion) to retrieve a Sprite for a character:
    • If emotion is null or empty, defaults to "Calm"
    • Returns the sprite for the requested emotion if exists
    • Falls back to default sprite if the requested emotion is missing
    • Returns null if character is not found
- Automatically builds dictionaries in Awake() for fast runtime access
- Designed to integrate with DialogueController or CharacterPortraitController

Usage:
- Call GetSprite("CharacterName", "EmotionName") to get the correct Sprite
- Supports default fallback logic for missing emotions
*/

public class CharacterDatabase : MonoBehaviour
{
    public List<CharacterDefinition> characters = new();

    private Dictionary<string, Sprite> defaultDict;
    private Dictionary<string, Dictionary<string, Sprite>> emotionDict;

    void Awake()
    {
        Build();
    }

    void Build()
    {
        defaultDict = new();
        emotionDict = new();

        foreach (var ch in characters)
        {
            if (string.IsNullOrEmpty(ch.character))
                continue;

            defaultDict[ch.character] = ch.defaultSprite;

            var inner = new Dictionary<string, Sprite>();

            if (ch.emotions != null)
            {
                foreach (var e in ch.emotions)
                {
                    if (e == null || string.IsNullOrEmpty(e.emotion) || e.sprite == null)
                        continue;

                    inner[e.emotion] = e.sprite;
                }
            }

            emotionDict[ch.character] = inner;
        }
    }

    public Sprite GetSprite(string character, string emotion)
    {
        if (string.IsNullOrEmpty(character))
            return null;

        // Если emotion пустая → используем Calm
        if (string.IsNullOrEmpty(emotion))
            emotion = "Calm";

        // 1) пробуем эмоцию
        if (emotionDict.TryGetValue(character, out var ed) &&
            ed != null &&
            ed.TryGetValue(emotion, out var sprite))
        {
            return sprite;
        }

        // 2) если Calm нет — default
        if (defaultDict.TryGetValue(character, out var def))
            return def;

        return null;
    }

}
