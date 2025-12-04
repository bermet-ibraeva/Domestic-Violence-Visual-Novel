using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string episodeJsonPath; // например: "Episodes/episode_1"
    public int chapterNumber;      // 1, 2, 3...
    public string currentNodeId;   // например: "scene_1_start"
}

public static class SaveManager
{
    private const string SaveKey = "VN_SAVE";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public static SaveData Load()
    {
        if (!HasSave()) return null;

        string json = PlayerPrefs.GetString(SaveKey);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void Delete()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}
