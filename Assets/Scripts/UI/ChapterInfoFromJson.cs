using UnityEngine;
using TMPro;

[System.Serializable]
public class EpisodeHeader
{
    public string episode;   // поле "episode" из JSON, например "Тихие крики"
}

public class ChapterInfoFromJson : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI chapterNumberText; 
    public TextMeshProUGUI chapterTitleText;  

    /// <summary>
    /// Выводит главу и название эпизода на основе сейва
    /// </summary>
    public void ShowFromSave(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("SaveData is null в ChapterInfoFromJson");
            return;
        }

        // Загружаем TextAsset из Resources
        TextAsset jsonAsset = Resources.Load<TextAsset>(saveData.episodeJsonPath);

        if (jsonAsset == null)
        {
            Debug.LogError(
                $"JSON НЕ найден в Resources: {saveData.episodeJsonPath}\n" +
                $"Убедись, что файл лежит по пути: Assets/Resources/{saveData.episodeJsonPath}.json\n" +
                $"И загружается как Resources.Load<TextAsset>(\"{saveData.episodeJsonPath}\")"
            );

            return;
        }

        EpisodeHeader header = null;

        try
        {
            header = JsonUtility.FromJson<EpisodeHeader>(jsonAsset.text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка парсинга JSON: {ex.Message}");
            return;
        }

        if (header == null)
        {
            Debug.LogError("FromJson вернул null — JSON структура неправильная");
            return;
        }

        // Устанавливаем UI
        if (chapterNumberText != null)
            chapterNumberText.text = $"Глава {saveData.chapterNumber}";

        if (chapterTitleText != null)
            chapterTitleText.text = header.episode;
    }
}