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

    private NoteData currentNote;
    private NotesDatabase database;
    private SaveData save;

    private void Start()
    {
        save = SaveManager.Instance.Data;

        if (save == null)
        {
            Debug.LogError("[NoteDetail] SaveData is NULL");
            return;
        }

        UpdateStaticTexts();
        LoadDatabase();
        LoadNote();
        BindButtons();
    }

    private void LoadDatabase()
    {
        if (notesJson == null)
        {
            Debug.LogError("notesJson is NULL");
            return;
        }

        database = JsonUtility.FromJson<NotesDatabase>(notesJson.text);

        if (database == null)
            Debug.LogError("Failed to parse notes JSON");
    }

    private void LoadNote()
    {
        string selectedNoteId = NoteSession.SelectedNoteId;

        if (database == null)
        {
            Debug.LogError("[NoteDetail] Database is NULL");
            return;
        }

        currentNote = database.GetNoteById(selectedNoteId);

        if (currentNote == null)
        {
            Debug.LogError("Note not found");
            return;
        }

        MarkNoteAsRead(currentNote.noteId);
        RefreshUI();
    }

    private void RefreshUI()
    {
        string title = LocalizationManager.Instance.GetText("Notes", currentNote.titleKey);
        string content = LocalizationManager.Instance.GetText("Notes", currentNote.textKey);

        noteTitleText.text = title;
        noteContentText.text = FormatText(content);

        if (readTimeText != null)
            readTimeText.text = CalculateReadTime(content);

        SetupImage();
        SetupTestButton();
    }

    private string FormatText(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return "";

        string[] paragraphs = raw.Split(new string[] { "\n\n" }, System.StringSplitOptions.None);

        for (int i = 0; i < paragraphs.Length; i++)
        {
            paragraphs[i] = paragraphs[i].Trim() + "\n\n";
        }

        return string.Join("", paragraphs);
    }

    private string CalculateReadTime(string text)
    {
        if (string.IsNullOrEmpty(text))
            return LocalizationManager.Instance.GetText("Notes", "read_time_1");

        string cleanText = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", "");

        int wordCount = cleanText.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length;
        int minutes = Mathf.CeilToInt(wordCount / 200f);

        if (minutes <= 1)
            return LocalizationManager.Instance.GetText("Notes", "read_time_1");

        return LocalizationManager.Instance.GetText("Notes", "read_time_n")
            .Replace("{0}", minutes.ToString());
    }

    private void SetupImage()
    {
        if (string.IsNullOrEmpty(currentNote.image))
        {
            noteImage.sprite = fallbackImage;
            return;
        }

        Debug.Log("Loading image: " + currentNote.image);

        Sprite loaded = Resources.Load<Sprite>(currentNote.image);

        Debug.Log(loaded == null ? "NOT FOUND" : "LOADED");

        noteImage.sprite = loaded != null ? loaded : fallbackImage;
    }

    private void SetupTestButton()
    {
        bool hasTest = !string.IsNullOrEmpty(currentNote.testId);

        openTestButton.gameObject.SetActive(hasTest);

        if (hasTest && openTestButtonText != null)
        {
            openTestButtonText.text =
                LocalizationManager.Instance.GetText("Notes", "note_open_test");
        }
    }

    private void BindButtons()
    {
        openTestButton.onClick.RemoveAllListeners();
        openTestButton.onClick.AddListener(OpenTest);

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(GoBack);
    }

    private void OpenTest()
    {
        TestSession.SelectedTestId = currentNote.testId;
        SceneManager.LoadScene(testSceneName);
        Debug.Log("Opening test: " + currentNote.testId);
    }

    private void GoBack()
    {
        SceneManager.LoadScene(notesSceneName);
    }

    private void MarkNoteAsRead(string noteId)
    {
        NoteState note = save.GetOrCreateNote(noteId);

        if (!note.isRead)
        {
            save.MarkNoteAsRead(noteId); 

            if (!note.rewardClaimed)
            {
                note.rewardClaimed = true;

                save.sparksTotal += 2;

                if (TempGameContext.CurrentEpisode != null)
                    TempGameContext.CurrentEpisode.sparks += 2;
            }

            SaveManager.Instance.Save();
        }
    }


    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(Language lang)
    {
        UpdateStaticTexts();
        RefreshUI();
    }

    private void UpdateStaticTexts()
    {
        if (pageTitleText != null)
            pageTitleText.text = LocalizationManager.Instance.GetText("Notes", "notes_page_title");
    }
}