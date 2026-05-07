using UnityEngine;
using System.Collections.Generic;

public static class SaveSystem
{
    private const string KEY = "VN_SAVE";

    public static void Save(SaveData data)
    {
        if (data == null)
            return;

        Normalize(data);

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public static SaveData Load()
    {
        if (!PlayerPrefs.HasKey(KEY))
            return null;

        string json = PlayerPrefs.GetString(KEY);

        if (string.IsNullOrEmpty(json))
            return null;

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
            return null;

        Normalize(data);

        return data;
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
    }

    public static SaveData CreateNew()
    {
        SaveData data = new SaveData();

        Normalize(data);

        Save(data);

        return data;
    }

    public static void Normalize(SaveData data)
    {
        if (data.completedEpisodes == null)
            data.completedEpisodes = new List<string>();

        if (data.appliedEffectNodes == null)
            data.appliedEffectNodes = new List<string>();

        if (data.shownNotificationIds == null)
            data.shownNotificationIds = new List<string>();

        if (data.notes == null)
            data.notes = new List<NoteState>();

        if (data.testsBest == null)
            data.testsBest = new List<TestBestScore>();
    }
}