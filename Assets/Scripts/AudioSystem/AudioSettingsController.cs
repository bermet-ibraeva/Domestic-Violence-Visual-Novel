using UnityEngine;

public class AudioSettingsController : MonoBehaviour
{
    private void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.ApplySavedSettings();
    }

    public void SetBackgroundVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBackgroundVolume(value);
    }

    public void SetSceneVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSceneVolume(value);
    }
}