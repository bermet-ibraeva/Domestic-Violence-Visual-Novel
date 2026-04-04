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
    [SerializeField] private Button openTestButton;
    [SerializeField] private Button backButton;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text openTestButtonText;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private GameObject errorRoot;
    [SerializeField] private TMP_Text errorText;

    [Header("Scene Names")]
    [SerializeField] private string notesSceneName = "NotesListPage";
    [SerializeField] private string testSceneName = "TestPage";


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

        SetupTestButton();
    }

    private void BindButtons()
    {
        if (openTestButton != null)
        {
            openTestButton.onClick.RemoveAllListeners();
            openTestButton.onClick.AddListener(OpenTest);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
        }
    }

    private void SetupTestButton()
    {
        if (openTestButton == null || currentNote == null)
            return;

        bool hasTest = !string.IsNullOrEmpty(currentNote.testId);
        openTestButton.gameObject.SetActive(hasTest);

        if (!hasTest)
            return;

        if (openTestButtonText != null)
            openTestButtonText.text = "Пройти тест";
    }

    private void OpenTest()
    {
        if (currentNote == null)
            return;

        if (string.IsNullOrEmpty(currentNote.testId))
            return;

        TestSession.SelectedTestId = currentNote.testId;
        SceneManager.LoadScene(testSceneName);
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

                int reward = 2;

                save.sparksTotal += reward;
                TempGameContext.CurrentEpisode.sparks += reward;

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