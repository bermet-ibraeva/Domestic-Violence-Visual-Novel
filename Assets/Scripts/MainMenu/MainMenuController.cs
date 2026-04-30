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
        if (!SaveSystem.HasSave())
        {
            currentSave = null;
            return;
        }

        currentSave = SaveSystem.Load();

        if (currentSave == null)
        {
            Debug.LogWarning("[MainMenu] Save corrupted → resetting");
            SaveSystem.Clear();
            currentSave = null;
        }
    }

    // ================= UI =================

    private void UpdateUI()
    {
        bool hasValidSave =
            currentSave != null &&
            !string.IsNullOrEmpty(currentSave.episodePath);

        // кнопка Play / Continue
        if (playButtonText != null)
        {
            string key = hasValidSave ? "continue" : "play";
            playButtonText.text = LocalizationManager.Instance.GetText("MainMenu", key);
        }

        if (chapterInfoUI == null)
        {
            Debug.LogError("[MainMenu] chapterInfoUI is NULL");
            return;
        }

        //  если save нет или он сломан → показываем дефолт
        if (!hasValidSave)
        {
            ShowDefaultEpisode();
            return;
        }

        // 🔹 пробуем загрузить эпизод из save
        EpisodeData episode = EpisodeLoader.LoadEpisode(
            currentSave.episodePath,
            out _,
            out _,
            out _
        );

        if (episode == null)
        {
            Debug.LogWarning("[MainMenu] Failed to load saved episode → fallback");
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
            Debug.LogError("[MainMenu] Default episode failed to load");
            return;
        }

        string startNode = GetStartNode(episode);

        if (string.IsNullOrEmpty(startNode))
        {
            Debug.LogError("[MainMenu] Cannot determine start node");
            return;
        }

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
        //  если нет save → создаём новый
        if (currentSave == null || string.IsNullOrEmpty(currentSave.episodePath))
        {
            SaveSystem.StartEpisode(episodePath);
        }

        SceneManager.LoadScene("EpisodePage");
    }
}