using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    public ChapterInfoFromJson chapterInfoUI;
    public TextMeshProUGUI playButtonText;

    [Header("Сцена эпизода")]
    public string sceneToLoad = "Episode1";

    private SaveData currentSave;
    private bool isNewGame;

    private const string DEFAULT_EPISODE_PATH = "Episodes/episode_1";
    private const string DEFAULT_NODE_ID = "E01_S01_start";
    private const int DEFAULT_CHAPTER = 1;

    void Start()
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
        if (playButtonText != null)
            playButtonText.text = isNewGame ? "ИГРАТЬ" : "ПРОДОЛЖИТЬ";

        if (chapterInfoUI != null)
            chapterInfoUI.ShowFromSave(currentSave);
        else
            Debug.LogError("chapterInfoUI is NULL. Assign it in Inspector.");
    }

    private SaveData CreateDefaultSave()
    {
        return new SaveData
        {
            episodePath   = DEFAULT_EPISODE_PATH,
            currentNodeId = DEFAULT_NODE_ID,
            chapterNumber = DEFAULT_CHAPTER
        };
    }

    private bool IsDefaultSave(SaveData data)
    {
        if (data == null) return true;

        return data.episodePath   == DEFAULT_EPISODE_PATH &&
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