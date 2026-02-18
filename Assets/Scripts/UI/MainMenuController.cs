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

    // Значения "по умолчанию" для НОВОЙ игры
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

            // ⬇⬇⬇ КЛЮЧЕВОЙ МОМЕНТ ⬇⬇⬇
            if (IsDefaultSave(currentSave))
            {
                // Сейв есть, но он на самом старте → считаем НОВОЙ ИГРОЙ
                playButtonText.text = "ИГРАТЬ";
            }
            else
            {
                // Есть реальный прогресс → можно продолжать
                playButtonText.text = "ПРОДОЛЖИТЬ";
            }
        }
        else
        {
            // Вообще нет сейва → создаём дефолт, но считаем это НОВОЙ ИГРОЙ
            currentSave = CreateDefaultSave();
            playButtonText.text = "ИГРАТЬ";
        }

        if (chapterInfoUI != null)
            chapterInfoUI.ShowFromSave(currentSave);
    }

    // Создаём объект сейва для НОВОЙ игры
    private SaveData CreateDefaultSave()
    {
        return new SaveData
        {
            episodePath   = DEFAULT_EPISODE_PATH,
            currentNodeId = DEFAULT_NODE_ID,
            chapterNumber = DEFAULT_CHAPTER
        };
    }

    // Проверяем, "на самом ли старте" сейв
    private bool IsDefaultSave(SaveData data)
    {
        if (data == null) return true;

        return data.episodePath   == DEFAULT_EPISODE_PATH &&
               data.currentNodeId == DEFAULT_NODE_ID &&
               data.chapterNumber == DEFAULT_CHAPTER;
    }

    public void OnPlayButton()
    {
        // Если на кнопке "ИГРАТЬ" → начинаем НОВУЮ игру,
        // независимо от того, что там сейчас лежит в сейве
        if (playButtonText.text == "ИГРАТЬ")
        {
            SaveSystem.Clear();               // старый прогресс полностью убрать
            currentSave = CreateDefaultSave();
            SaveSystem.Save(currentSave);
        }

        // Передаём сейв в сцену эпизода
        TempGameContext.saveToLoad = currentSave;

        StartCoroutine(LoadSceneNextFrame(sceneToLoad));
    }

    private IEnumerator LoadSceneNextFrame(string sceneName)
    {
        yield return null;
        SceneManager.LoadScene(sceneName);
    }
}
