using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Managers")]
    [SerializeField] private BackgroundMusicManager backgroundMusicManager;
    [SerializeField] private SceneMusicManager sceneMusicManager;
    [SerializeField] private SFXManager sfxManager;

    private const string BG_VOLUME_KEY = "BG_VOLUME";
    private const string SCENE_VOLUME_KEY = "SCENE_VOLUME";

    public float BackgroundVolume => PlayerPrefs.GetFloat(BG_VOLUME_KEY, 1f);
    public float SceneVolume => PlayerPrefs.GetFloat(SCENE_VOLUME_KEY, 1f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        AutoAssignManagers();
    }

    private void Start()
    {
        ApplySavedSettings();
    }

    private void AutoAssignManagers()
    {
        if (backgroundMusicManager == null)
            backgroundMusicManager = FindFirstObjectByType<BackgroundMusicManager>();

        if (sceneMusicManager == null)
            sceneMusicManager = FindFirstObjectByType<SceneMusicManager>();

        if (sfxManager == null)
            sfxManager = FindFirstObjectByType<SFXManager>();
    }

    public void ApplySavedSettings()
    {
        SetBackgroundVolume(PlayerPrefs.GetFloat(BG_VOLUME_KEY, 1f));
        SetSceneVolume(PlayerPrefs.GetFloat(SCENE_VOLUME_KEY, 1f));
    }

    public void SetBackgroundVolume(float value)
    {
        float clamped = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BG_VOLUME_KEY, clamped);
        PlayerPrefs.Save();

        if (backgroundMusicManager != null)
            backgroundMusicManager.SetVolume(clamped);
    }

    public void SetSceneVolume(float value)
    {
        float clamped = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SCENE_VOLUME_KEY, clamped);
        PlayerPrefs.Save();

        if (sceneMusicManager != null)
            sceneMusicManager.SetVolume(clamped);
    }

    public void PlayDefaultMusic()
    {
        if (backgroundMusicManager != null)
            backgroundMusicManager.PlayDefault();
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicManager != null)
            backgroundMusicManager.Stop();
    }

    public void PlaySceneMusic(string id)
    {
        if (sceneMusicManager != null)
            sceneMusicManager.PlaySceneMusicById(id);
    }

    public void StopSceneMusic()
    {
        if (sceneMusicManager != null)
            sceneMusicManager.StopSceneMusic();
    }

    public void ApplyNodeAudio(AudioData audioData)
    {
        if (sceneMusicManager != null)
            sceneMusicManager.ApplyNodeAudio(audioData);
    }

    public void PlaySFX(string id)
    {
        if (sfxManager != null)
            sfxManager.PlaySFX(id);
    }
}