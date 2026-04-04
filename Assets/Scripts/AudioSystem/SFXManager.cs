using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioEntry[] sfxLibrary;

    private Dictionary<string, AudioClip> sfxMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxMap = new Dictionary<string, AudioClip>();

        foreach (var entry in sfxLibrary)
        {
            if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
            {
                sfxMap[entry.id] = entry.clip;
            }
        }
    }

    public void PlaySFX(string id)
    {
        if (sfxSource == null) return;

        foreach (var entry in sfxLibrary)
        {
            if (entry != null && entry.id == id && entry.clip != null)
            {
                sfxSource.PlayOneShot(entry.clip, entry.volume);
                return;
            }
        }
    }
}