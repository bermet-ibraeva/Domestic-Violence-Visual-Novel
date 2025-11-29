using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundSprite
    {
        public string id;       // "evening_street", "school_yard" и т.д.
        public Sprite sprite;   // сам спрайт
    }

    public Image backgroundImage;               // Image на Canvas
    public List<BackgroundSprite> backgrounds;  // список id → sprite

    private Dictionary<string, Sprite> bgDict;

    void Awake()
    {
        bgDict = new Dictionary<string, Sprite>();

        foreach (var b in backgrounds)
        {
            if (b.sprite != null)
            {
                bgDict[b.id] = b.sprite;
            }
        }
    }

    public void SetBackground(string id)
    {
        Debug.Log("[BG] SetBackground called with id = " + id);

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[BG] Empty id");
            return;
        }

        if (bgDict.TryGetValue(id, out Sprite sprite))
        {
            backgroundImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning("[BG] Background not found: " + id);
        }
    }
}
