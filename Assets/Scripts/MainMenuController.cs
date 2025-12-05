using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    public ChapterInfoFromJson chapterInfoUI;
    public TextMeshProUGUI playButtonText;

    [Header("Сцена с эпизодом")]
    public string sceneToLoad = "Episode1";  

    private SaveData currentSave;

    void Start()
    {
        if (SaveManager.HasSave())
        {
            currentSave = SaveManager.Load();

            if (currentSave == null)
            {
                Debug.LogWarning("HasSave() = true, но Load() вернул null. Создаем дефолтный сейв.");
                CreateDefaultSave();
                SaveManager.Save(currentSave);
            }

            if (playButtonText != null)
                playButtonText.text = "ПРОДОЛЖИТЬ";
        }
        else
        {
            CreateDefaultSave();

            if (playButtonText != null)
                playButtonText.text = "ИГРАТЬ";
        }

        if (chapterInfoUI != null)
            chapterInfoUI.ShowFromSave(currentSave);
    }

    private void CreateDefaultSave()
    {
        currentSave = new SaveData
        {
            episodeJsonPath = "Episodes/episode_1", // теперь путь для Resources
            chapterNumber = 1,
            currentNodeId = "scene_1_start"
        };
    }

    public void OnPlayButton()
    {
        TempGameContext.saveToLoad = currentSave;

        StopAllCoroutines();
        StartCoroutine(LoadSceneNextFrame(sceneToLoad));
    }

    private IEnumerator LoadSceneNextFrame(string sceneName)
    {
        yield return null;
        SceneManager.LoadScene(sceneName);
    }
}
