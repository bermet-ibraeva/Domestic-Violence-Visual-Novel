using UnityEngine;
using TMPro;

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

        if (LocalizationManager.Instance == null)
            return;

        int number = ExtractEpisodeNumber(episode.episodeId);

        // номер
        string label = LocalizationManager.Instance.GetText("MainMenu", "episode_label");
        chapterNumberText.text = string.Format(label, number);

        // название
        string key = $"episode_{number}_title";
        chapterTitleText.text = LocalizationManager.Instance.GetText("MainMenu", key);
    }

    int ExtractEpisodeNumber(string episodeId)
    {
        if (string.IsNullOrEmpty(episodeId))
            return 1;

        string numberPart = episodeId.Replace("E", "");

        if (int.TryParse(numberPart, out int number))
            return number;

        Debug.LogWarning($"[ChapterInfo] Failed to parse episode number from: {episodeId}");
        return 1;
    }
}