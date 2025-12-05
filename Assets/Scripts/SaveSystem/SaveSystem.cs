using UnityEngine;

public static class SaveSystem
{
    private const string KEY = "VN_SAVE";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(KEY);
    }

    public static SaveData Load()
    {
        if (!HasSave()) return null;

        string json = PlayerPrefs.GetString(KEY);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
    }
}
