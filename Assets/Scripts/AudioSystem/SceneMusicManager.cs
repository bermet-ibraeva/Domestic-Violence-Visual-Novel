using System.Collections.Generic;
using UnityEngine;

public class SceneMusicManager : MonoBehaviour
{
    public static SceneMusicManager Instance;

    [SerializeField] private AudioSource sceneMusicSource;
    [SerializeField] private AudioEntry[] musicLibrary;

    private Dictionary<string, AudioClip> musicMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildMusicMap();
    }

    private void BuildMusicMap()
    {
        musicMap = new Dictionary<string, AudioClip>();

        if (musicLibrary == null)
            return;

        foreach (var entry in musicLibrary)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.id) && entry.clip != null)
            {
                musicMap[entry.id] = entry.clip;
            }
        }
    }

    public void ApplyNodeAudio(AudioData audio)
    {
        if (audio == null)
            return;

        if (audio.stopMusic)
        {
            StopSceneMusic();
            return;
        }

        if (audio.changeMusic && !string.IsNullOrEmpty(audio.musicId))
        {
            PlaySceneMusicById(audio.musicId);
        }
    }

    public void PlaySceneMusicById(string id)
    {
        if (sceneMusicSource == null)
            return;

        if (!musicMap.TryGetValue(id, out AudioClip clip) || clip == null)
            return;

        if (sceneMusicSource.clip == clip && sceneMusicSource.isPlaying)
            return;

        sceneMusicSource.clip = clip;
        sceneMusicSource.loop = true;
        sceneMusicSource.Play();
    }

    public void StopSceneMusic()
    {
        if (sceneMusicSource == null)
            return;

        sceneMusicSource.Stop();
        sceneMusicSource.clip = null;
    }

    public void SetVolume(float volume)
    {
        if (sceneMusicSource == null)
            return;

        sceneMusicSource.volume = Mathf.Clamp01(volume);
    }
}