using System.Collections;
using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public static BackgroundMusicController Instance;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioClip defaultMusic;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    private const string VolumeKey = "BG_VOLUME";

    private Coroutine fadeCoroutine;

    private float currentVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadSettings();

        PlayDefault();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        currentVolume = 1f;

        if (musicSource != null)
        {
            musicSource.volume = currentVolume;
        }
    }

    public void PlayDefault()
    {
        Play(defaultMusic);
    }

    public void Play(AudioClip clip)
    {
        if (musicSource == null || clip == null)
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

        PlayerPrefs.SetFloat(
            VolumeKey,
            currentVolume);

        if (musicSource != null)
        {
            musicSource.volume = currentVolume;
        }
    }

    public float GetVolume()
    {
        return currentVolume;
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
        }
    }
}