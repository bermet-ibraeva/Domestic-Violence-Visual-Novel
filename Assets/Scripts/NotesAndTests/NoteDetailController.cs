using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NoteDetailController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TextAsset notesJson;

    [Header("UI")]
    [SerializeField] private TMP_Text pageTitleText;
    [SerializeField] private TMP_Text noteTitleText;
    [SerializeField] private TMP_Text noteContentText;
    [SerializeField] private TMP_Text readTimeText;

    [SerializeField] private Image noteImage;

    [SerializeField] private Button openTestButton;
    [SerializeField] private TMP_Text openTestButtonText;

    [SerializeField] private Button backButton;

    [Header("Image")]
    [SerializeField] private Sprite fallbackImage;

    [Header("Scene Names")]
    [SerializeField] private string notesSceneName = "NotesListPage";
    [SerializeField] private string testSceneName = "TestPage";

    private NotesDatabase database;
    private NoteData currentNote;

    // ================= UNITY =================
    private void Start()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[NoteDetail] SaveManager is NULL");
            return;
        }

        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("[NoteDetail] LocalizationManager is NULL");
            return;
        }

        LoadDatabase();
        BindButtons();
        LoadNote();
        UpdateStaticTexts();
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }

        if (openTestButton != null)
        {
            openTestButton.onClick.RemoveListener(OpenTest);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoBack);
        }
    }


    // ================= DATABASE =================
    private void LoadDatabase()
    {
        if (notesJson == null)
        {
            Debug.LogError("[NoteDetail] notesJson is NULL");
            return;
        }

        database = JsonUtility.FromJson<NotesDatabase>(notesJson.text);

        if (database == null)
        {
            Debug.LogError("[NoteDetail] Failed to parse notes JSON");
        }
    }

    // ================= NOTE =================
    private void LoadNote()
    {
        if (database == null)
        {
            Debug.LogError("[NoteDetail] Database is NULL");
            return;
        }

        string selectedNoteId = NoteSession.SelectedNoteId;

        if (string.IsNullOrEmpty(selectedNoteId))
        {
            Debug.LogError("[NoteDetail] SelectedNoteId is NULL");
            return;
        }

        currentNote = database.GetNoteById(selectedNoteId);

        if (currentNote == null)
        {
            Debug.LogError("[NoteDetail] Note not found: " + selectedNoteId);
            return;
        }

        MarkNoteAsRead(currentNote.noteId);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (currentNote == null)
            return;

        string title =
            LocalizationManager.Instance.GetText("Notes", currentNote.titleKey);

        string content =
            LocalizationManager.Instance.GetText("Notes", currentNote.textKey);

        if (noteTitleText != null)
        {
            noteTitleText.text = title;
        }

        if (noteContentText != null)
        {
            noteContentText.text = FormatText(content);
        }

        if (readTimeText != null)
        {
            readTimeText.text = CalculateReadTime(content);
        }

        SetupImage();
        SetupTestButton();
    }

    // ================= NOTE STATE =================
    private void MarkNoteAsRead(string noteId)
    {
        if (string.IsNullOrEmpty(noteId))
            return;

        NoteState note = SaveManager.Instance.Data.GetOrCreateNote(noteId);

        if (note == null)
        {
            Debug.LogError("[NoteDetail] Failed to get/create note state");
            return;
        }

        // already read -> no duplicate rewards
        if (note.isRead)
            return;

        SaveManager.Instance.Data.MarkNoteAsRead(noteId);

        // first read reward
        if (!note.readRewardClaimed)
        {
            note.readRewardClaimed = true;

            if (StatSystem.Instance != null)
            {
                StatSystem.Instance.AddEpisodeReward(2);
            }
            else
            {
                SaveManager.Instance.Data.AddSparks(2);
            }
        }

        SaveManager.Instance.AutoSave();

        Debug.Log("[NoteDetail] Note marked as read: " + noteId);
    }

    // ================= UI HELPERS =================
    private void SetupImage()
    {
        if (noteImage == null)
            return;

        if (string.IsNullOrEmpty(currentNote.image))
        {
            noteImage.sprite = fallbackImage;
            return;
        }

        Sprite loaded = Resources.Load<Sprite>(currentNote.image);

        noteImage.sprite = loaded != null
            ? loaded
            : fallbackImage;
    }

    private void SetupTestButton()
    {
        if (openTestButton == null)
            return;

        bool hasTest = !string.IsNullOrEmpty(currentNote.testId);

        openTestButton.gameObject.SetActive(hasTest);

        if (hasTest && openTestButtonText != null)
        {
            openTestButtonText.text =
                LocalizationManager.Instance.GetText(
                    "Notes",
                    "note_open_test"
                );
        }
    }

    private string FormatText(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return "";

        string[] paragraphs = raw.Split(
            new string[] { "\n\n" },
            System.StringSplitOptions.None
        );

        for (int i = 0; i < paragraphs.Length; i++)
        {
            paragraphs[i] = paragraphs[i].Trim() + "\n\n";
        }

        return string.Join("\n\n", paragraphs);
    }

    private string CalculateReadTime(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return LocalizationManager.Instance.GetText(
                "Notes",
                "read_time_1"
            );
        }

        string cleanText =
            System.Text.RegularExpressions.Regex.Replace(
                text,
                "<.*?>",
                ""
            );

        int wordCount = cleanText.Split(
            ' ',
            System.StringSplitOptions.RemoveEmptyEntries
        ).Length;

        int minutes = Mathf.CeilToInt(wordCount / 200f);

        if (minutes <= 1)
        {
            return LocalizationManager.Instance.GetText(
                "Notes",
                "read_time_1"
            );
        }

        return LocalizationManager.Instance
            .GetText("Notes", "read_time_n")
            .Replace("{0}", minutes.ToString());
    }

    // ================= BUTTONS =================
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

    private void OpenTest()
    {
        if (currentNote == null)
            return;

        if (string.IsNullOrEmpty(currentNote.testId))
            return;

        TestSession.SelectedTestId = currentNote.testId;

        Debug.Log("[NoteDetail] Opening test: " + currentNote.testId);

        SceneManager.LoadScene(testSceneName);
    }

    private void GoBack()
    {
        SceneManager.LoadScene(notesSceneName);
    }

    // ================= LOCALIZATION =================
    private void OnLanguageChanged(Language lang)
    {
        UpdateStaticTexts();
        RefreshUI();
    }

    private void UpdateStaticTexts()
    {
        if (pageTitleText != null)
        {
            pageTitleText.text =
                LocalizationManager.Instance.GetText(
                    "Notes",
                    "notes_page_title"
                );
        }
    }
}