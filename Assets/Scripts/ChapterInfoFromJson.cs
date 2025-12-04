using UnityEngine;
using TMPro;
using System.IO;  // ВАЖНО: для File.ReadAllText

[System.Serializable]
public class EpisodeHeader
{
    public string episode;   // поле "episode" из JSON, например "Тихие крики"
}

public class ChapterInfoFromJson : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI chapterNumberText; // "Глава 1"
    public TextMeshProUGUI chapterTitleText;  // "Тихие крики"

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

        // Сборка полного пути: StreamingAssets + относительный путь из сейва
        string fullPath = Path.Combine(Application.streamingAssetsPath, saveData.episodeJsonPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"JSON файл НЕ найден в StreamingAssets: {fullPath}");
            return;
        }

        string jsonText;
        try
        {
            jsonText = File.ReadAllText(fullPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка чтения JSON ({fullPath}): {ex.Message}");
            return;
        }

        EpisodeHeader header;
        try
        {
            header = JsonUtility.FromJson<EpisodeHeader>(jsonText);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка парсинга EpisodeHeader: {ex.Message}");
            return;
        }

        if (header == null)
        {
            Debug.LogError("JsonUtility.FromJson вернул null (EpisodeHeader)");
            return;
        }

        if (chapterNumberText != null)
            chapterNumberText.text = $"Глава {saveData.chapterNumber}";

        if (chapterTitleText != null)
            chapterTitleText.text = header.episode;
    }
}
