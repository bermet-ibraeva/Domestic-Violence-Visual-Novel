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
    [SerializeField] private TMP_Text readTimeText;
    [SerializeField] private Image noteImage;
    [SerializeField] private Button openTestButton;
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
        save = SaveSystem.Load();
        LoadDatabase();
        LoadNote();
        BindButtons();
    }

    private void LoadDatabase()
    {
        database = JsonUtility.FromJson<NotesDatabase>(notesJson.text);
    }

    private void LoadNote()
    {
        string selectedNoteId = NoteSession.SelectedNoteId;

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
        noteTitleText.text = currentNote.title;
        noteContentText.text = FormatText(currentNote.text);
        
        if (readTimeText != null)
            readTimeText.text = CalculateReadTime(currentNote.text);

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
            return "1 мин";

        // убираем rich text теги (<b>, <i>, <color>)
        string cleanText = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", "");

        // считаем слова
        int wordCount = cleanText.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length;

        // считаем минуты
        int minutes = Mathf.CeilToInt(wordCount / 200f);

        if (minutes == 1)
            return "1 мин";
        else
            return $"{minutes} мин";
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
            note.isRead = true;

            if (!note.rewardClaimed)
            {
                note.rewardClaimed = true;

                save.sparksTotal += 2;
                TempGameContext.CurrentEpisode.sparks += 2;
            }

            SaveSystem.Save(save);
        }
    }
}