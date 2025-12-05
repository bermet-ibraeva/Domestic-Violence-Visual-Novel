using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string episodePath;
    public string nodeId;
    public int chapterNumber;
}

public static class SaveSystem
{
    const string KEY = "VN_SAVE";

    public static void Save()
    {
        SaveData data = new SaveData
        {
            episodePath = GameContext.currentEpisodePath,
            nodeId = GameContext.currentNodeId,
            chapterNumber = GameContext.chapterNumber
        };

        PlayerPrefs.SetString(KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static bool HasSave() => PlayerPrefs.HasKey(KEY);

    public static SaveData Load()
    {
        if (!HasSave()) return null;

        return JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(KEY));
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
    }
}
