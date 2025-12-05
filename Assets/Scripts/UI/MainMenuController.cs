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

    void Start()
    {
        // =========================
        // 1. Проверяем наличие сейва
        // =========================
        if (SaveManager.HasSave())
        {
            currentSave = SaveManager.Load();

            if (currentSave == null)
            {
                Debug.LogWarning("HasSave = true, но Load() вернул null. Пересоздаем сейв.");
                CreateDefaultSave();
                SaveManager.Save(currentSave);
            }

            playButtonText.text = "ПРОДОЛЖИТЬ";
        }
        else
        {
            // Нет сохранений → игра начинается как новая
            CreateDefaultSave();
            playButtonText.text = "ИГРАТЬ";
        }

        // Показ информации о главе
        if (chapterInfoUI != null)
            chapterInfoUI.ShowFromSave(currentSave);
    }

    // ======================================================
    // СОЗДАНИЕ ДЕФОЛТНОГО СЕЙВА (для новой игры)
    // ======================================================
    private void CreateDefaultSave()
    {
        currentSave = new SaveData
        {
            episodePath = "Episodes/episode_1", // путь в Resources (БЕЗ .json)
            chapterNumber = 1,
            currentNodeId = "scene_1_start"
        };
    }

    // ======================================================
    // КНОПКА "ИГРАТЬ" / "ПРОДОЛЖИТЬ"
    // ======================================================
    public void OnPlayButton()
    {
        // Если кнопка "ИГРАТЬ" — значит пользователь хочет начать заново.
        if (playButtonText.text == "ИГРАТЬ")
        {
            SaveManager.Delete();  // Удаляем старый сейв
            CreateDefaultSave();   // Создаем новый
            SaveManager.Save(currentSave);
        }

        // Передаём сейв в следующую сцену
        TempGameContext.saveToLoad = currentSave;

        StartCoroutine(LoadSceneNextFrame(sceneToLoad));
    }

    private IEnumerator LoadSceneNextFrame(string sceneName)
    {
        yield return null;
        SceneManager.LoadScene(sceneName);
    }
}
