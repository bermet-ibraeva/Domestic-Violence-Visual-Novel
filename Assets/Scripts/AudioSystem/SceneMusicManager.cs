using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMusicController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;

    [Header("Music Library")]
    [SerializeField] private AudioEntry[] musicLibrary;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    private Dictionary<string, AudioClip> musicMap;

    private Coroutine fadeCoroutine;

    private float currentVolume = 1f;

    private void Awake()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        BuildMusicMap();
    }

    private void BuildMusicMap()
    {
        musicMap = new Dictionary<string, AudioClip>();

        if (musicLibrary == null)
            return;

        foreach (var entry in musicLibrary)
        {
            if (entry != null &&
                !string.IsNullOrEmpty(entry.id) &&
                entry.clip != null)
            {
                musicMap[entry.id] = entry.clip;
            }
        }
    }

    public void Initialize(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
        {
            musicSource.volume = currentVolume;
        }
    }

    public void ApplyNodeAudio(AudioData audio)
    {
        if (audio == null)
            return;

        if (audio.stopMusic)
        {
            StopSmooth();
            return;
        }

        if (audio.changeMusic &&
            !string.IsNullOrEmpty(audio.musicId))
        {
            PlayById(audio.musicId);
        }
    }

    public void PlayById(string id)
    {
        if (musicSource == null)
            return;

        if (!musicMap.TryGetValue(id, out AudioClip clip))
            return;

        if (musicSource.clip == clip &&
            musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;

        musicSource.volume = 0f;

        musicSource.Play();

        StartFade(currentVolume);
    }

    public void Stop()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        musicSource.clip = null;
    }

    public void StopSmooth()
    {
        if (musicSource == null ||
            !musicSource.isPlaying)
            return;

        StartFade(0f, true);
    }

    public void Pause()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void Resume()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
        {
            musicSource.volume = currentVolume;
        }
    }

    private void StartFade(
        float targetVolume,
        bool stopAfterFade = false)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(
            FadeMusic(
                targetVolume,
                fadeDuration,
                stopAfterFade));
    }

    private IEnumerator FadeMusic(
        float targetVolume,
        float duration,
        bool stopAfterFade)
    {
        float startVolume = musicSource.volume;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            musicSource.volume = Mathf.Lerp(
                startVolume,
                targetVolume,
                time / duration);

            yield return null;
        }

        musicSource.volume = targetVolume;

        if (stopAfterFade)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }
}