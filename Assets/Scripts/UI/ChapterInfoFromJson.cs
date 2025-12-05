using UnityEngine;
using TMPro;

[System.Serializable]
public class EpisodeHeader
{
    public string episode;   // название эпизода из JSON
}

public class ChapterInfoFromJson : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI chapterNumberText;
    public TextMeshProUGUI chapterTitleText;

    public void ShowFromSave(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("SaveData is null в ChapterInfoFromJson");
            return;
        }

        // Загружаем JSON по episodePath
        TextAsset jsonAsset = Resources.Load<TextAsset>(saveData.episodePath);

        if (jsonAsset == null)
        {
            Debug.LogError(
                $"JSON НЕ найден в Resources: {saveData.episodePath}\n" +
                $"Убедись, что файл лежит по пути: Assets/Resources/{saveData.episodePath}.json"
            );
            return;
        }

        EpisodeHeader header = JsonUtility.FromJson<EpisodeHeader>(jsonAsset.text);

        if (header == null)
        {
            Debug.LogError("Ошибка парсинга: header == null");
            return;
        }

        // Обновляем UI
        if (chapterNumberText != null)
            chapterNumberText.text = $"Глава {saveData.chapterNumber}";

        if (chapterTitleText != null)
            chapterTitleText.text = header.episode;
    }
}
