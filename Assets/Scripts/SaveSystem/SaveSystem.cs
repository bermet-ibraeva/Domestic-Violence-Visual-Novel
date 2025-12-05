using UnityEngine;

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

        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public static bool HasSave() => PlayerPrefs.HasKey(KEY);

    public static SaveData Load()
    {
        if (!HasSave())
            return null;

        return JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(KEY));
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
    }
}
