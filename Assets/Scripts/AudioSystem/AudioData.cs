using System;
using UnityEngine;

[System.Serializable]
public class AudioEntry
{
    public string id;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public float maxDuration = 0f;
}

[Serializable]
public class AudioData
{
    public bool changeMusic;
    public string musicId;
    public bool stopMusic;
    public bool playSFX;
    public string sfxId;
    public bool loopMusic = true;
}