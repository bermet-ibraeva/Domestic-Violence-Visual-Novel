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
            chapterNumberText.text = $"Эпизод {ExtractEpisodeNumber(episode.episodeId)}";
        if (chapterTitleText != null)
            chapterTitleText.text = episode.episodeTitle;
    }

    int ExtractEpisodeNumber(string episodeId)
    {
        if (string.IsNullOrEmpty(episodeId))
            return 1;

        // например "E02" → "02"
        string numberPart = episodeId.Replace("E", "");

        if (int.TryParse(numberPart, out int number))
            return number;

        Debug.LogWarning($"[ChapterInfo] Failed to parse episode number from: {episodeId}");
        return 1;
    }
}