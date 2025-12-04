using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    public ChapterInfoFromJson chapterInfoUI;
    public TextMeshProUGUI playButtonText;   // текст на кнопке "ИГРАТЬ / ПРОДОЛЖИТЬ"

    [Header("Сцена с эпизодом")]
    public string sceneToLoad = "Episode1";  // имя Scene

    private SaveData currentSave;

    void Start()
    {
        if (SaveManager.HasSave())
        {
            currentSave = SaveManager.Load();
            if (currentSave == null)
            {
                Debug.LogWarning("HasSave() = true, но Load() вернул null. Создаём дефолтный сейв.");
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
        // ВАЖНО: путь относительно StreamingAssets.
        // Если файл: Assets/StreamingAssets/Episode1.json → "Episode1.json"
        // Если:    Assets/StreamingAssets/Episodes/episode_1.json → "Episodes/episode_1.json"
        currentSave = new SaveData
        {
            episodeJsonPath = "Episode1.json",
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
