using UnityEngine;

public static class SaveManager
{
    private const string SaveKey = "VN_SAVE";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public static SaveData Load()
    {
        if (!HasSave())
            return null;

        string json = PlayerPrefs.GetString(SaveKey);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void Delete()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}
