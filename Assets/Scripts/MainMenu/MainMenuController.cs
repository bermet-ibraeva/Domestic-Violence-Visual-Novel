using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    public ChapterInfoFromJson chapterInfoUI;
    public TextMeshProUGUI playButtonText;
    [SerializeField] private LanguageSelectionController languageSelectionController;

    [Header("Scene")]
    public string sceneToLoad = "EpisodePage";

    private SaveData currentSave;
    private bool isNewGame;

    private const string DEFAULT_EPISODE_PATH = "Episodes/episode_1";
    private const string DEFAULT_NODE_ID = "E01_S01_start";
    private const int DEFAULT_CHAPTER = 1;

    private void Start()
    {
        InitializeSave();
        UpdateMainMenuUI();
        CheckFirstLaunchLanguageSelection();
    }

    private void InitializeSave()
    {
        if (SaveSystem.HasSave())
        {
            currentSave = SaveSystem.Load();

            if (currentSave == null)
            {
                Debug.LogWarning("Save exists, but Load() returned null. Recreating save.");
                currentSave = CreateDefaultSave();
                SaveSystem.Save(currentSave);
            }
        }
        else
        {
            currentSave = CreateDefaultSave();
            SaveSystem.Save(currentSave);
        }

        isNewGame = IsDefaultSave(currentSave);
    }

    private void UpdateMainMenuUI()
    {
        playButtonText.text = isNewGame
            ? LocalizationManager.Instance.GetText("MainMenu", "play")
            : LocalizationManager.Instance.GetText("MainMenu", "continue");

        if (chapterInfoUI != null)
            chapterInfoUI.ShowFromSave(currentSave);
        else
            Debug.LogError("chapterInfoUI is NULL. Assign it in Inspector.");
    }

    private void CheckFirstLaunchLanguageSelection()
    {
        if (LocalizationManager.Instance == null)
            return;

        if (LocalizationManager.Instance.IsFirstLaunch())
        {
            if (languageSelectionController != null)
                languageSelectionController.Show();
            else
                Debug.LogWarning("LanguageSelectionController is not assigned in MainMenuController.");
        }
    }

    private SaveData CreateDefaultSave()
    {
        return new SaveData
        {
            episodePath = DEFAULT_EPISODE_PATH,
            currentNodeId = DEFAULT_NODE_ID,
            chapterNumber = DEFAULT_CHAPTER
        };
    }

    private bool IsDefaultSave(SaveData data)
    {
        if (data == null) return true;

        return data.episodePath == DEFAULT_EPISODE_PATH &&
               data.currentNodeId == DEFAULT_NODE_ID &&
               data.chapterNumber == DEFAULT_CHAPTER;
    }

    public void OnPlayButton()
    {
        if (isNewGame)
        {
            currentSave = CreateDefaultSave();
            SaveSystem.Save(currentSave);
        }
        else
        {
            currentSave = SaveSystem.Load();

            if (currentSave == null)
            {
                currentSave = CreateDefaultSave();
                SaveSystem.Save(currentSave);
            }
        }

        TempGameContext.saveToLoad = currentSave;
        StartCoroutine(LoadSceneNextFrame(sceneToLoad));
    }

    private IEnumerator LoadSceneNextFrame(string sceneName)
    {
        yield return null;
        SceneManager.LoadScene(sceneName);
    }
}