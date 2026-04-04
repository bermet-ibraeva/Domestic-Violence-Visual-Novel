using System;
using UnityEngine;

[Serializable]
public class AudioEntry
{
    public string id;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

[Serializable]
public class AudioData
{
    public bool changeMusic;
    public string musicId;
    public bool stopMusic;
}