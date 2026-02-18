using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private List<BackgroundEntry> backgrounds;

    private Dictionary<string, Sprite> bgDict;

    private void Awake()
    {
        bgDict = new Dictionary<string, Sprite>();

        foreach (var b in backgrounds)
        {
            if (b == null || string.IsNullOrEmpty(b.key) || b.sprite == null)
                continue;

            if (!bgDict.ContainsKey(b.key))
                bgDict.Add(b.key, b.sprite);
        }
    }

    public void SetBackground(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (bgDict.TryGetValue(key, out var sprite))
        {
            backgroundImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"Background '{key}' not found in BackgroundController.");
        }
    }
}

[System.Serializable]
public class BackgroundEntry
{
    public string key;
    public Sprite sprite;
}
