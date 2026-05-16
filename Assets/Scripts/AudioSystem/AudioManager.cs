using UnityEngine;

/*
Audio System Architecture

The audio subsystem is responsible for managing background music,
scene-specific music, and sound effects throughout the application.
Its primary purpose is to provide centralized audio control while
maintaining separation between different types of sound playback.

The architecture is divided into several interconnected components:

* AudioManager
* BackgroundMusicController
* SceneMusicController
* SFXController
* AudioEntry
* AudioData

General Architecture
AudioManager acts as the central coordinator of the entire audio system.
It provides a global access point for audio operations and manages
communication between music controllers and sound effect controllers.

The subsystem separates audio into three categories:

1. Global background music;
2. Scene-specific music;
3. Sound effects (SFX).

This separation allows independent control of different audio layers
during gameplay.

Audio Data Structures
AudioEntry represents a reusable audio configuration containing:

* unique audio identifier;
* AudioClip reference;
* volume multiplier;
* optional maximum playback duration.

AudioData is used inside dialogue nodes and scene data to describe
audio-related actions during gameplay. The structure supports:

* changing scene music;
* stopping currently playing music;
* playing sound effects;
* configuring music looping behavior.

Runtime Lifecycle
AudioManager is initialized during scene startup and automatically
locates SceneMusicController and SFXController instances if references
have not been assigned manually.

During initialization:

1. saved volume settings are loaded from PlayerPrefs;
2. music and SFX controllers are initialized;
3. current volume values are applied to AudioSources.

AudioManager provides centralized runtime methods for:

* scene music playback;
* sound effect playback;
* volume management;
* pause/resume operations;
* node-based audio processing.

Scene Music System
Scene-specific music is managed through SceneMusicController.
This component stores a music library built from AudioEntry objects
mapped into runtime dictionaries for fast lookup.

Main responsibilities:

* loading music by identifier;
* switching tracks dynamically;
* fading music in/out smoothly;
* handling looping playback;
* ducking music volume during sound effects;
* pausing and resuming music.

When DialogueController processes a dialogue node containing audio data,
the node passes AudioData into AudioManager, which forwards music-related
operations to SceneMusicController.

Music transitions are implemented using Coroutines in order to:

* smoothly fade out previous tracks;
* switch clips;
* fade in new music without abrupt interruption.

If sound effects are played simultaneously, the controller temporarily
reduces scene music volume using a ducking system. After the effect
finishes, the original music volume is restored gradually.

Background Music System
BackgroundMusicController is responsible for persistent global menu music.
Unlike scene-specific music, this controller survives scene transitions
using DontDestroyOnLoad.

Main responsibilities:

* playing default menu music;
* smooth fade transitions;
* volume control;
* pause/resume support.

The controller automatically starts default background music during
application startup and prevents duplicate playback if the same track
is already active.

Sound Effects System
Sound effects are managed through SFXController.
The component stores a runtime dictionary of sound effect identifiers
mapped to AudioEntry objects for fast retrieval.

Main responsibilities:

* playing sound effects by identifier;
* applying independent SFX volume settings;
* limiting playback duration;
* stopping effects automatically when required.

Sound effects are triggered through:
AudioManager → SFXController → AudioSource.PlayOneShot()

This allows effects to overlap without interrupting currently playing
music tracks.

Volume Management
The subsystem stores separate volume settings for:

* scene music;
* background music;
* sound effects.

Volume values are stored locally using PlayerPrefs and restored during
application startup.

Each audio category can be controlled independently through:

* SetSceneVolume();
* SetSFXVolume();
* SetVolume().

Runtime Integration
The audio subsystem is integrated directly into:

* dialogue nodes;
* scene transitions;
* notifications;
* menu interfaces;
* gameplay events.

Instead of storing direct AudioClip references inside gameplay logic,
the application stores audio identifiers. During runtime,
these identifiers are resolved through the corresponding controller
dictionaries.

Example runtime flow:
Dialogue node
↓
AudioData
↓
AudioManager
↓
SceneMusicController / SFXController
↓
AudioSource playback

Advantages of the Architecture
The implemented audio architecture provides:

* centralized audio management;
* separation of music and sound effects;
* smooth music transitions;
* reusable audio libraries;
* persistent audio settings;
* scalable node-based audio triggering;
* simplified integration with gameplay systems.

Overall, the audio subsystem functions as a modular runtime audio
management layer integrated with dialogue progression, scene management,
and user interaction systems throughout the application.
*/
 
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