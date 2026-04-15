using UnityEngine;
using TMPro;
using System;


public class ChapterInfoFromJson : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI chapterNumberText;
    public TextMeshProUGUI chapterTitleText;

    public void Show(SaveData saveData, EpisodeData episode)
    {
        if (saveData == null || episode == null)
        {
            Debug.LogError("SaveData или EpisodeData null в ChapterInfo");
            return;
        }

        if (chapterNumberText != null)
            chapterNumberText.text = $"Эпизод {ExtractEpisodeNumber(saveData.episodePath)}";
        if (chapterTitleText != null)
            chapterTitleText.text = episode.episodeTitle;
    }

    int ExtractEpisodeNumber(string path)
    {
        if (string.IsNullOrEmpty(path))
            return 1;

        string fileName = path.Substring(path.LastIndexOf('_') + 1);

        if (int.TryParse(fileName, out int number))
            return number;

        Debug.LogWarning($"[ChapterInfo] Failed to parse episode number from: {path}");
        return 1;
    }
}