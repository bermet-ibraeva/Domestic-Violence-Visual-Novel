using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Library")]
    [SerializeField] private AudioEntry[] sfxLibrary;

    private Dictionary<string, AudioEntry> sfxMap;

    private float currentVolume = 1f;

    private void Awake()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }

        BuildSFXMap();
    }

    private void BuildSFXMap()
    {
        sfxMap = new Dictionary<string, AudioEntry>();

        if (sfxLibrary == null)
            return;

        foreach (var entry in sfxLibrary)
        {
            if (entry != null &&
                !string.IsNullOrEmpty(entry.id) &&
                entry.clip != null)
            {
                sfxMap[entry.id] = entry;
            }
        }
    }

    public void Initialize(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
    }

    public void Play(string id)
    {
        if (sfxSource == null)
            return;

        if (!sfxMap.TryGetValue(id, out AudioEntry entry))
            return;

        sfxSource.PlayOneShot(
            entry.clip,
            entry.volume * currentVolume);
    }

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
    }
}