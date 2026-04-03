using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NoteDetailController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TextAsset notesJson;

    [Header("UI")]
    [SerializeField] private TMP_Text noteTitleText;
    [SerializeField] private TMP_Text noteContentText;
    [SerializeField] private Button openQuizButton;
    [SerializeField] private Button backButton;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text openQuizButtonText;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private GameObject errorRoot;
    [SerializeField] private TMP_Text errorText;

    [Header("Scene Names")]
    [SerializeField] private string notesSceneName = "NotesScene";
    [SerializeField] private string quizSceneName = "QuizScene";

    private NoteData currentNote;
    private NotesDatabase database;
    private SaveData save;

    private void Start()
    {
        save = SaveSystem.Load();

        if (save == null)
        {
            ShowError("Не удалось загрузить сохранение.");
            Debug.LogError("[NoteDetailController] SaveData is null.");
            return;
        }

        LoadDatabase();
        LoadNote();
        BindButtons();
    }

    private void LoadDatabase()
    {
        if (notesJson == null)
        {
            ShowError("Файл заметок не назначен.");
            Debug.LogError("[NoteDetailController] notesJson is not assigned.");
            return;
        }

        database = JsonUtility.FromJson<NotesDatabase>(notesJson.text);

        if (database == null)
        {
            ShowError("Не удалось загрузить базу заметок.");
            Debug.LogError("[NoteDetailController] Failed to parse NotesDatabase.");
        }
    }

    private void LoadNote()
    {
        string selectedNoteId = NoteSession.SelectedNoteId;

        if (string.IsNullOrEmpty(selectedNoteId))
        {
            ShowError("Заметка не выбрана.");
            Debug.LogWarning("[NoteDetailController] SelectedNoteId is null or empty.");
            return;
        }

        if (database == null)
        {
            ShowError("База заметок не найдена.");
            Debug.LogError("[NoteDetailController] Notes Database is null.");
            return;
        }

        currentNote = database.GetNoteById(selectedNoteId);

        if (currentNote == null)
        {
            ShowError("Не удалось загрузить заметку.");
            Debug.LogWarning($"[NoteDetailController] Note not found for id: {selectedNoteId}");
            return;
        }

        MarkNoteAsRead(currentNote.noteId);
        ShowContent();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (noteTitleText != null)
            noteTitleText.text = currentNote.title;

        if (noteContentText != null)
        {
            noteContentText.richText = true;
            noteContentText.text = currentNote.text;
        }

        SetupQuizButton();
    }

    private void BindButtons()
    {
        if (openQuizButton != null)
        {
            openQuizButton.onClick.RemoveAllListeners();
            openQuizButton.onClick.AddListener(OpenQuiz);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
        }
    }

    private void SetupQuizButton()
    {
        if (openQuizButton == null || currentNote == null)
            return;

        bool hasQuiz = !string.IsNullOrEmpty(currentNote.quizId);
        openQuizButton.gameObject.SetActive(hasQuiz);

        if (!hasQuiz)
            return;

        if (openQuizButtonText != null)
            openQuizButtonText.text = "Пройти тест";
    }

    private void OpenQuiz()
    {
        if (currentNote == null)
        {
            Debug.LogWarning("[NoteDetailController] currentNote is null.");
            return;
        }

        if (string.IsNullOrEmpty(currentNote.quizId))
        {
            Debug.LogWarning("[NoteDetailController] currentNote.quizId is empty.");
            return;
        }

        QuizSession.SelectedQuizId = currentNote.quizId;
        SceneManager.LoadScene(quizSceneName);
    }

    private void GoBack()
    {
        SceneManager.LoadScene(notesSceneName);
    }

    private void MarkNoteAsRead(string noteId)
    {
        if (string.IsNullOrEmpty(noteId))
            return;

        NoteState note = save.GetOrCreateNote(noteId);

        if (!note.isRead)
        {
            note.isRead = true;

            if (!note.rewardClaimed)
            {
                note.rewardClaimed = true;
                save.sparksTotal += 2;
                save.episodeSparks += 2;

                Debug.Log("[NoteDetailController] +2 sparks for note: " + noteId);
            }

            SaveSystem.Save(save);
        }
    }

    private void ShowContent()
    {
        if (contentRoot != null)
            contentRoot.SetActive(true);

        if (errorRoot != null)
            errorRoot.SetActive(false);
    }

    private void ShowError(string message)
    {
        if (contentRoot != null)
            contentRoot.SetActive(false);

        if (errorRoot != null)
            errorRoot.SetActive(true);

        if (errorText != null)
            errorText.text = message;
    }
}