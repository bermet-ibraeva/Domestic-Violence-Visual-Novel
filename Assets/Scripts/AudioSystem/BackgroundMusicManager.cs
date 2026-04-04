using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip defaultMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayDefault();
    }

    public void PlayDefault()
    {
        if (musicSource == null || defaultMusic == null) return;

        if (musicSource.clip == defaultMusic && musicSource.isPlaying)
            return;

        musicSource.clip = defaultMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void Stop()
    {
        if (musicSource == null) return;

        musicSource.Stop();
    }

    public void SetVolume(float volume)
    {
        if (musicSource == null) return;

        musicSource.volume = Mathf.Clamp01(volume);
    }
}