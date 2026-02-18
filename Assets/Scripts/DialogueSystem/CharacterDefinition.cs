using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EmotionSprite
{
    public string emotion;   // "Nervous"
    public Sprite sprite;
}

[Serializable]
public class CharacterDefinition
{
    public string character;         // "Ainaz"
    public Sprite defaultSprite;
    public List<EmotionSprite> emotions;
}
