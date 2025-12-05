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
        if (SaveSystem.HasSave())
        {
            currentSave = SaveSystem.Load();

            if (currentSave == null)
            {
                Debug.LogWarning("Save exists, but Load() returned null. Recreating save.");
                CreateDefaultSave();
                SaveSystem.Save(currentSave);
            }

            playButtonText.text = "ПРОДОЛЖИТЬ";
        }
        else
        {
            // Нет сохранений → новая игра
            CreateDefaultSave();
            playButtonText.text = "ИГРАТЬ";
        }

        if (chapterInfoUI != null)
            chapterInfoUI.ShowFromSave(currentSave);
    }

    // ======================================================
    // СОЗДАЁМ ДЕФОЛТНЫЙ СЕЙВ ДЛЯ НОВОЙ ИГРЫ
    // ======================================================
    private void CreateDefaultSave()
    {
        currentSave = new SaveData
        {
            episodePath = "Episodes/episode_1",   // Resources path
            currentNodeId = "scene_1_start",
            chapterNumber = 1
        };
    }

    // ======================================================
    // НАЖАТИЕ КНОПКИ "ИГРАТЬ / ПРОДОЛЖИТЬ"
    // ======================================================
    public void OnPlayButton()
    {
        // Если это новая игра
        if (playButtonText.text == "ИГРАТЬ")
        {
            SaveSystem.Clear();   // Удаляем старый прогресс полностью
            CreateDefaultSave();  // Создаём новый сейв
            SaveSystem.Save(currentSave);
        }

        // Передаём сейв в диалоговую сцену
        TempGameContext.saveToLoad = currentSave;

        StartCoroutine(LoadSceneNextFrame(sceneToLoad));
    }

    private IEnumerator LoadSceneNextFrame(string sceneName)
    {
        yield return null;
        SceneManager.LoadScene(sceneName);
    }
}
