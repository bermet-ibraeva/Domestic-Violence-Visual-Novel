using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Controllers")]
    [SerializeField] private SceneMusicController sceneMusic;

    [SerializeField] private SFXController sfx;

    private const string SCENE_VOLUME_KEY = "SCENE_VOLUME";
    private const string SFX_VOLUME_KEY = "SFX_VOLUME";

    public float SceneVolume =>
        PlayerPrefs.GetFloat(SCENE_VOLUME_KEY, 1f);

    public float SFXVolume =>
        PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        AutoAssignControllers();
    }

    private void Start()
    {
        InitializeControllers();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private void AutoAssignControllers()
    {
        if (sceneMusic == null)
        {
            sceneMusic =
                FindFirstObjectByType<SceneMusicController>();
        }

        if (sfx == null)
        {
            sfx =
                FindFirstObjectByType<SFXController>();
        }
    }

    private void InitializeControllers()
    {
        if (sceneMusic != null)
        {
            sceneMusic.Initialize(SceneVolume);
        }

        if (sfx != null)
        {
            sfx.Initialize(SFXVolume);
        }
    }

    // =========================
    // Scene Music
    // =========================
    public void ApplyNodeAudio(AudioData audioData)
    {
        if (sceneMusic != null)
        {
            sceneMusic.ApplyNodeAudio(audioData);
        }
        if (audioData.playSFX && !string.IsNullOrEmpty(audioData.sfxId))
        {
            PlaySFX(audioData.sfxId);
        }
    }

    public void PlaySceneMusic(string id, bool loop = true)
    {
        if (sceneMusic != null)
        {
            sceneMusic.PlayById(id, loop: true);
        }
    }

    public void StopSceneMusic()
    {
        if (sceneMusic != null)
        {
            sceneMusic.StopSmooth();
        }
    }

    public void SetSceneVolume(float value)
    {
        float clamped = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(
            SCENE_VOLUME_KEY,
            clamped);

        if (sceneMusic != null)
        {
            sceneMusic.SetVolume(clamped);
        }
    }

    // =========================
    // SFX
    // =========================
    public void PlaySFX(string id)
    {
        if (sceneMusic != null)
        {
            sceneMusic.DuckVolume();
        }

        if (sfx != null)
        {
            sfx.Play(id);
        }
    }

    public void SetSFXVolume(float value)
    {
        float clamped = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(
            SFX_VOLUME_KEY,
            clamped);

        if (sfx != null)
        {
            sfx.SetVolume(clamped);
        }
    }

    // =========================
    // Global Controls
    // =========================

    public void PauseAll()
    {
        sceneMusic?.Pause();
    }

    public void ResumeAll()
    {
        sceneMusic?.Resume();
    }
}