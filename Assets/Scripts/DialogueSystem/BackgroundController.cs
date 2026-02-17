using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundSprite
    {
        public Sprite sprite;
    }

    public Image backgroundImage;
    public List<BackgroundSprite> backgrounds;

    private Dictionary<string, Sprite> bgDict = new();
    private string currentBackgroundName;

    private void Awake()
    {
        bgDict.Clear();

        foreach (var b in backgrounds)
        {
            if (b == null || b.sprite == null) continue;

            string key = b.sprite.name; // <-- имя спрайта

            // перезапишет если одинаковые спрайты
            bgDict[key] = b.sprite;
        }
    }

    public void SetBackground(string nameFromJson)
    {
        if (string.IsNullOrEmpty(nameFromJson) || backgroundImage == null)
            return;

        if (nameFromJson == currentBackgroundName)
            return;

        if (bgDict.TryGetValue(nameFromJson, out var sprite))
        {
            backgroundImage.sprite = sprite;
            currentBackgroundName = nameFromJson;
        }
        else
        {
            Debug.LogWarning($"[BG] Background not found by name: {nameFromJson}. " +
                             "Проверь: sprite.name должен совпадать с JSON.");
        }
    }
}
