using UnityEngine;
using TMPro;
using System;

[Serializable]
public class EpisodeHeader
{
    public string episodeId;
    public string episodeTitle;  
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

        TextAsset jsonAsset = Resources.Load<TextAsset>(saveData.episodePath);
        if (jsonAsset == null)
        {
            Debug.LogError($"JSON НЕ найден в Resources: {saveData.episodePath}");
            return;
        }

        EpisodeHeader header = JsonUtility.FromJson<EpisodeHeader>(jsonAsset.text);
        if (header == null)
        {
            Debug.LogError("Ошибка парсинга: header == null");
            return;
        }

        if (chapterNumberText != null)
            chapterNumberText.text = $"Эпизод {saveData.chapterNumber}";

        if (chapterTitleText != null)
            chapterTitleText.text = header.episodeTitle; 
    }
}