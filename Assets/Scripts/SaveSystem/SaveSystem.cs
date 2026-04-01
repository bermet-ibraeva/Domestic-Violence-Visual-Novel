using UnityEngine;

public static class SaveSystem
{
    private const string KEY = "VN_SAVE";

    // Сохранение
    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    // Проверка
    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(KEY);
    }

    // Загрузка (безопасная)
    public static SaveData Load()
    {
        if (!HasSave())
        {
            return CreateNew();
        }

        string json = PlayerPrefs.GetString(KEY);

        if (string.IsNullOrEmpty(json))
        {
            return CreateNew();
        }

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            return CreateNew();
        }

        return data;
    }

    // Новый сейв
    public static SaveData CreateNew()
    {
        SaveData data = new SaveData();
        Save(data);
        return data;
    }

    // Сброс
    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
    }
}