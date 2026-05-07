using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private ChapterInfoFromJson chapterInfoUI;
    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private Button playButton;

    [Header("Default Episode")]
    [SerializeField] private string episodePath = "Demos/demo_1";

    [Header("Scene Names")]
    [SerializeField] private string episodeSceneName = "EpisodePage";

    private SaveData currentSave;

    private void Start()
    {
        Time.timeScale = 1f;

        LoadSave();
        UpdateUI();
        BindButton();
    }

    // ================= SAVE =================

    private void LoadSave()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[MainMenu] SaveManager is NULL");
            return;
        }

        currentSave = SaveManager.Instance.Data;

        // corrupted save fallback
        if (currentSave == null)
        {
            Debug.LogWarning("[MainMenu] Save is NULL -> resetting");

            SaveManager.Instance.Clear();

            currentSave = SaveManager.Instance.Data;
        }
    }

    // ================= UI =================

    private void UpdateUI()
    {
        bool hasValidSave =
            currentSave != null &&
            !string.IsNullOrEmpty(currentSave.episodePath);

        // Play / Continue button
        if (playButtonText != null &&
            LocalizationManager.Instance != null)
        {
            string key = hasValidSave
                ? "continue"
                : "play";

            playButtonText.text =
                LocalizationManager.Instance.GetText(
                    "MainMenu",
                    key
                );
        }

        if (chapterInfoUI == null)
        {
            Debug.LogError("[MainMenu] chapterInfoUI is NULL");
            return;
        }

        // no save -> show default episode preview
        if (!hasValidSave)
        {
            ShowDefaultEpisode();
            return;
        }

        EpisodeData episode = EpisodeLoader.LoadEpisode(
            currentSave.episodePath,
            out _,
            out _,
            out _
        );

        // broken episode path fallback
        if (episode == null)
        {
            Debug.LogWarning(
                "[MainMenu] Failed to load saved episode"
            );

            ShowDefaultEpisode();
            return;
        }

        chapterInfoUI.Show(currentSave, episode);
    }

    private void ShowDefaultEpisode()
    {
        EpisodeData episode = EpisodeLoader.LoadEpisode(
            episodePath,
            out _,
            out _,
            out _
        );

        if (episode == null)
        {
            Debug.LogError(
                "[MainMenu] Default episode failed to load"
            );

            return;
        }

        string startNode = GetStartNode(episode);

        if (string.IsNullOrEmpty(startNode))
        {
            Debug.LogError(
                "[MainMenu] Start node not found"
            );

            return;
        }

        // temporary preview save
        SaveData temp = new SaveData
        {
            episodePath = episodePath,
            currentNodeId = startNode
        };

        chapterInfoUI.Show(temp, episode);
    }

    private string GetStartNode(EpisodeData episode)
    {
        if (episode == null || episode.scenes == null)
            return null;

        foreach (var scene in episode.scenes)
        {
            if (!string.IsNullOrEmpty(scene.startNode))
                return scene.startNode;
        }

        return null;
    }

    // ================= BUTTON =================

    private void BindButton()
    {
        if (playButton == null)
        {
            Debug.LogError("[MainMenu] PlayButton is NULL");
            return;
        }

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(OnPlayPressed);
    }

    private void OnPlayPressed()
    {
        bool hasSave =
            currentSave != null &&
            !string.IsNullOrEmpty(currentSave.episodePath);

        // start new game only if save doesn't exist
        if (!hasSave)
        {
            SaveManager.Instance.StartEpisode(episodePath);
        }

        SceneManager.LoadScene(episodeSceneName);
    }
}