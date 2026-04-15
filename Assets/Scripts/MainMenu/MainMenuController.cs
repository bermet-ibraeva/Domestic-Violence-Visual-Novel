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

    private SaveData currentSave;

    private const string DEFAULT_EPISODE_PATH = "Episodes/episode_1";
    private const string DEFAULT_NODE_ID = "E01_S01_start";
    private const int DEFAULT_CHAPTER = 1;

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
        if (SaveSystem.HasSave())
        {
            currentSave = SaveSystem.Load();

            if (currentSave == null)
            {
                Debug.LogWarning("[MainMenu] Save corrupted. Clearing.");
                SaveSystem.Clear();
                currentSave = null;
            }
        }
        else
        {
            currentSave = null;
        }
    }

    // ================= UI =================

    private void UpdateUI()
    {
        // 🔹 кнопка
        if (playButtonText != null)
            playButtonText.text = currentSave != null ? "ПРОДОЛЖИТЬ" : "ИГРАТЬ";

        if (chapterInfoUI == null)
        {
            Debug.LogError("[MainMenu] chapterInfoUI is NULL.");
            return;
        }

        // 🔹 если нет save → показываем дефолтный эпизод
        if (currentSave == null)
        {
            EpisodeData episode = EpisodeLoader.LoadEpisode(
                DEFAULT_EPISODE_PATH,
                out _,
                out _,
                out _
            );

            if (episode != null)
            {
                SaveData temp = new SaveData
                {
                    episodePath = DEFAULT_EPISODE_PATH,
                    currentNodeId = DEFAULT_NODE_ID,
                    chapterNumber = DEFAULT_CHAPTER
                };

                chapterInfoUI.Show(temp, episode);
            }

            return;
        }

        // 🔹 если есть save → показываем текущий прогресс
        EpisodeData currentEpisode = EpisodeLoader.LoadEpisode(
            currentSave.episodePath,
            out _,
            out _,
            out _
        );

        if (currentEpisode == null)
        {
            Debug.LogError($"[MainMenu] Failed to load episode: {currentSave.episodePath}");
            return;
        }

        chapterInfoUI.Show(currentSave, currentEpisode);
    }

    // ================= BUTTON =================

    private void BindButton()
    {
        if (playButton == null)
        {
            Debug.LogError("[MainMenu] PlayButton is not assigned!");
            return;
        }

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(OnPlayPressed);
    }

    private void OnPlayPressed()
    {
        // 🔹 если нет save → начинаем новую игру
        if (!SaveSystem.HasSave())
        {
            SaveSystem.StartEpisode(DEFAULT_EPISODE_PATH);
        }

        SceneManager.LoadScene("EpisodePage");
    }
}