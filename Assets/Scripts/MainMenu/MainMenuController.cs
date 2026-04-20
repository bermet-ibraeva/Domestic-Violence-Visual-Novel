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
        if (SaveSystem.HasSave())
        {
            currentSave = SaveSystem.Load();

            if (currentSave == null)
            {
                Debug.LogWarning("[MainMenu] Save corrupted. Clearing.");
                SaveSystem.Clear();
                currentSave = null;
                return;
            }

            if (currentSave.episodePath != episodePath)
            {
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
        // кнопка
        if (playButtonText != null)
            playButtonText.text = (currentSave != null && currentSave.episodePath == episodePath) ? "ПРОДОЛЖИТЬ" : "ИГРАТЬ";

        if (chapterInfoUI == null)
        {
            Debug.LogError("[MainMenu] chapterInfoUI is NULL.");
            return;
        }

        // если нет save → показываем дефолтный эпизод
        if (currentSave == null)
        {
            EpisodeData episode = EpisodeLoader.LoadEpisode(
                episodePath,
                out _,
                out _,
                out _
            );

            if (episode != null)
            {
                string startNode = null;    
                
                foreach (var scene in episode.scenes)
                {
                    if (!string.IsNullOrEmpty(scene.startNode))
                    {
                        startNode = scene.startNode;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(startNode))
                {
                    Debug.LogError("[MainMenu] Cannot determine start node.");
                    return;
                }

                SaveData temp = new SaveData
                {
                    episodePath = episodePath,
                    currentNodeId = startNode
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
        // если нет save или хочешь всегда запускать выбранный эпизод
        if (currentSave == null || currentSave.episodePath != episodePath)
        {
            SaveSystem.Clear();
            SaveSystem.StartEpisode(episodePath);
        }

        SceneManager.LoadScene("EpisodePage");
    }
}