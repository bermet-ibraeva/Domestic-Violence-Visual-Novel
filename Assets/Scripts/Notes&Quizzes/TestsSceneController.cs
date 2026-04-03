using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestsSceneController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TextAsset quizzesJson;

    [Header("UI")]
    [SerializeField] private Transform cardsRoot;
    [SerializeField] private TestCardUI cardPrefab;
    [SerializeField] private Button backButton;

    [Header("Scene Names")]
    [SerializeField] private string notesSceneName = "NotesScene";
    [SerializeField] private string quizSceneName = "QuizScene";

    [Header("Optional UI")]
    [SerializeField] private GameObject emptyRoot;
    [SerializeField] private TMP_Text emptyText;

    private SaveData save;
    private QuizDatabase database;

    private void Start()
    {
        save = SaveSystem.Load();
        LoadDatabase();
        BindButtons();
        RefreshList();
    }

    private void LoadDatabase()
    {
        if (quizzesJson == null)
        {
            Debug.LogError("[TestsSceneController] quizzesJson is missing.");
            return;
        }

        database = JsonUtility.FromJson<QuizDatabase>(quizzesJson.text);

        if (database == null || database.quizzes == null)
        {
            Debug.LogError("[TestsSceneController] Failed to parse quizzesJson.");
            database = new QuizDatabase
            {
                quizzes = new List<QuizData>()
            };
        }
    }

    private void BindButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
        }
    }

    public void RefreshList()
    {
        ClearCards();

        if (database == null || database.quizzes == null)
            return;

        List<QuizData> availableQuizzes = GetAvailableQuizzes();

        if (availableQuizzes.Count == 0)
        {
            ShowEmpty("Пока нет доступных тестов.");
            return;
        }

        HideEmpty();

        foreach (QuizData quiz in availableQuizzes)
        {
            TestBestScore best = save.GetOrCreateTest(quiz.quizId);

            TestCardUI card = Instantiate(cardPrefab, cardsRoot);
            card.Setup(quiz, best.bestScore, this);
        }
    }

    private List<QuizData> GetAvailableQuizzes()
    {
        List<QuizData> result = new List<QuizData>();

        foreach (QuizData quiz in database.quizzes)
        {
            if (string.IsNullOrEmpty(quiz.noteId))
                continue;

            NoteState note = save.GetOrCreateNote(quiz.noteId);

            if (note.isUnlocked)
                result.Add(quiz);
        }

        return result;
    }

    private void ClearCards()
    {
        if (cardsRoot == null)
            return;

        for (int i = cardsRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(cardsRoot.GetChild(i).gameObject);
        }
    }

    public void OpenQuiz(string quizId)
    {
        if (string.IsNullOrEmpty(quizId))
        {
            Debug.LogWarning("[TestsSceneController] quizId is null or empty.");
            return;
        }

        QuizSession.SelectedQuizId = quizId;
        SceneManager.LoadScene(quizSceneName);
    }

    private void GoBack()
    {
        SceneManager.LoadScene(notesSceneName);
    }

    private void ShowEmpty(string message)
    {
        if (emptyRoot != null)
            emptyRoot.SetActive(true);

        if (emptyText != null)
            emptyText.text = message;
    }

    private void HideEmpty()
    {
        if (emptyRoot != null)
            emptyRoot.SetActive(false);
    }
}