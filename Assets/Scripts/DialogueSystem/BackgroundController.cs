using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundSprite
    {
        public string id;      // "1", "2", "3", "4", "5"
        public Sprite sprite;
    }

    public Image backgroundImage;
    public List<BackgroundSprite> backgrounds;

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
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[BG] Empty id");
            return;
        }

        if (bgDict.TryGetValue(id, out Sprite sprite))
        {
            if (backgroundImage != null)
                backgroundImage.sprite = sprite;
            else
                Debug.LogWarning("[BG] backgroundImage is NULL");
        }
        else
        {
            Debug.LogWarning("[BG] Background not found: " + id);
        }
    }
}
