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
    private Coroutine duckCoroutine;
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

        if (audio.changeMusic && !string.IsNullOrEmpty(audio.musicId))
        {
            PlayById(audio.musicId, audio.loopMusic);
        }
    }

    public void PlayById(string id, bool loop)
    {
        if (musicSource == null)
            return;

        if (!musicMap.TryGetValue(id, out AudioClip clip))
        {
            Debug.LogWarning($"[SceneMusic] Music ID not found: {id}");
            return;
        }

        if (musicSource.clip == clip &&
                musicSource.isPlaying)
            return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(ChangeMusicRoutine(clip, loop));
    }

    public void Stop()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        musicSource.volume = 0f;
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

        fadeCoroutine = StartCoroutine(FadeMusic(targetVolume, fadeDuration, stopAfterFade));
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
            musicSource.volume = Mathf.SmoothStep(startVolume, targetVolume, time / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;

        if (stopAfterFade)
        {
            musicSource.Stop();
            musicSource.volume = 0f;
            musicSource.clip = null;
        }
    }

    private IEnumerator ChangeMusicRoutine(AudioClip newClip, bool loop)
    {
        float startVolume = musicSource.volume;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            musicSource.volume = Mathf.SmoothStep(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 0f;
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            musicSource.volume = Mathf.SmoothStep(0f, currentVolume, time / fadeDuration);
            yield return null;
        }

        musicSource.volume = currentVolume;
    }

    public void DuckVolume(float duckVolume = 0.40f, float duration = 0.15f)
    {
        if (duckCoroutine != null)
        {
            StopCoroutine(duckCoroutine);
        }

        duckCoroutine =
            StartCoroutine(
                DuckRoutine(duckVolume, duration));
    }

    private IEnumerator DuckRoutine(float duckVolume, float duration)
    {
        float originalVolume = currentVolume;

        musicSource.volume =
            originalVolume * duckVolume;

        yield return new WaitForSeconds(duration);

        float time = 0f;

        float startVolume = musicSource.volume;

        while (time < duration)
        {
            time += Time.deltaTime;

            musicSource.volume =
                Mathf.Lerp(
                    startVolume,
                    originalVolume,
                    time / duration);

            yield return null;
        }

        musicSource.volume = originalVolume;
    }
}