using System.Collections.Generic;
using System.Collections;
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
        {
            Debug.LogWarning(
                $"[SFX] Sound ID not found: {id}");

            return;
        }

        if (entry.clip == null)
            return;

        sfxSource.PlayOneShot(entry.clip, entry.volume * currentVolume);

        if (entry.maxDuration > 0f)
        {
            StartCoroutine(
                StopSFXAfterDelay(entry.maxDuration));
        }

    }

    private IEnumerator StopSFXAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        sfxSource.Stop();
    }

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
    }
}