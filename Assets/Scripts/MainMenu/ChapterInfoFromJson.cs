using UnityEngine;
using TMPro;

public class ChapterInfoFromJson : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI chapterNumberText;
    [SerializeField] private TextMeshProUGUI chapterTitleText;

    public void Show(SaveData save, EpisodeData episode)
    {
        if (episode == null)
        {
            Debug.LogError("[ChapterInfo] EpisodeData is NULL");
            return;
        }

        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("[ChapterInfo] LocalizationManager is NULL");
            return;
        }

        int number = ExtractEpisodeNumber(episode.episodeId);

        // Episode number
        string label =
            LocalizationManager.Instance.GetText(
                "MainMenu",
                "episode_label"
            );

        if (chapterNumberText != null)
        {
            chapterNumberText.text =
                string.Format(label, number);
        }

        // Episode title
        string key = $"episode_{number}_title";

        if (chapterTitleText != null)
        {
            chapterTitleText.text =
                LocalizationManager.Instance.GetText(
                    "MainMenu",
                    key
                );
        }
    }

    private int ExtractEpisodeNumber(string episodeId)
    {
        if (string.IsNullOrEmpty(episodeId))
            return 1;

        string numberPart = episodeId.Replace("E", "");

        if (int.TryParse(numberPart, out int number))
            return number;

        Debug.LogWarning(
            $"[ChapterInfo] Failed to parse episode number from: {episodeId}"
        );

        return 1;
    }
}