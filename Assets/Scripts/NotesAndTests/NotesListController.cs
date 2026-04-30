using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NotesListController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TextAsset notesJson;

    [Header("UI")]
    [SerializeField] private Transform cardsParent;
    [SerializeField] private GameObject noteCardPrefab;

    [Header("Scene Names")]
    [SerializeField] private string noteDetailSceneName = "NoteDetailPage";

    [Header("Optional")]
    [SerializeField] private bool showOnlyUnlocked = true;

    [Header("Texts")]
    [SerializeField] private TMP_Text pageTitleText;
    [SerializeField] private TMP_Text introText;

    private readonly List<GameObject> spawnedCards = new();
    private NotesDatabase database;
    private SaveData save;

    // ================= INIT =================

    private void Start()
    {
        save = SaveManager.Instance.Data;

        if (save == null)
        {
            Debug.LogError("[NotesList] SaveManager Data is NULL");
            return;
        }

        LoadDatabase();
        UpdateTexts();
        BuildNotesList();
    }

    private void LoadDatabase()
    {
        if (notesJson == null)
        {
            Debug.LogError("[NotesList] notesJson is not assigned.");
            return;
        }

        database = JsonUtility.FromJson<NotesDatabase>(notesJson.text);

        if (database == null)
        {
            Debug.LogError("[NotesList] Failed to parse notes database.");
        }
    }

    // ================= BUILD =================

    public void BuildNotesList()
    {
        if (database == null)
        {
            Debug.LogError("[NotesList] Database is null.");
            return;
        }

        if (save == null)
        {
            Debug.LogError("[NotesList] Save is null.");
            return;
        }

        if (cardsParent == null || noteCardPrefab == null)
        {
            Debug.LogError("[NotesList] UI references missing.");
            return;
        }

        ClearCards();

        List<NoteData> notes = database.notes;

        if (notes == null || notes.Count == 0)
        {
            Debug.LogWarning("[NotesList] No notes found.");
            return;
        }

        notes.Sort((a, b) => a.order.CompareTo(b.order));

        bool hasUnlocked = false;

        foreach (var note in notes)
        {
            if (note == null) continue;

            if (showOnlyUnlocked && !IsNoteUnlocked(note))
                continue;

            CreateCard(note);
            hasUnlocked = true;
        }

        // fallback → показываем все locked если нет unlocked
        if (!hasUnlocked)
        {
            foreach (var note in notes)
            {
                if (note == null) continue;
                CreateCard(note);
            }
        }
    }

    private void CreateCard(NoteData note)
    {
        GameObject cardObject = Instantiate(noteCardPrefab, cardsParent);
        spawnedCards.Add(cardObject);

        NoteCardUI cardUI = cardObject.GetComponent<NoteCardUI>();

        if (cardUI == null)
        {
            Debug.LogError($"[NotesList] NoteCardUI missing: {note.noteId}");
            return;
        }

        NoteState state = save.GetNote(note.noteId);

        if (state == null)
        {
            state = new NoteState
            {
                noteId = note.noteId,
                isUnlocked = false,
                isRead = false,
                rewardClaimed = false
            };
        }

        cardUI.Setup(note, this, state);
    }

    // ================= LOGIC =================

    private bool IsNoteUnlocked(NoteData note)
    {
        if (note == null)
            return false;

        NoteState state = save.GetNote(note.noteId);

        return state != null && state.isUnlocked;
    }

    private bool IsTestPassed(string testId)
    {
        if (string.IsNullOrEmpty(testId))
            return false;

        TestBestScore test = save.GetOrCreateTest(testId);
        return test.bestScore > 0;
    }

    // ================= NAVIGATION =================

    public void OpenNote(string noteId)
    {
        if (string.IsNullOrEmpty(noteId))
        {
            Debug.LogWarning("[NotesList] Empty noteId.");
            return;
        }

        NoteSession.SelectedNoteId = noteId;
        SceneManager.LoadScene(noteDetailSceneName);
    }

    // ================= UI =================

    private void ClearCards()
    {
        for (int i = cardsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(cardsParent.GetChild(i).gameObject);
        }

        spawnedCards.Clear();
    }

    public void RefreshList()
    {
        if (database != null)
            BuildNotesList();
    }

    private void UpdateTexts()
    {
        if (LocalizationManager.Instance == null)
            return;

        if (pageTitleText != null)
            pageTitleText.text = LocalizationManager.Instance.GetText("Notes", "notes_page_title");

        if (introText != null)
            introText.text = LocalizationManager.Instance.GetText("Notes", "notes_intro");
    }

    // ================= EVENTS =================

    private void OnEnable()
    {
        save = SaveManager.Instance.Data;

        if (database != null)
            RefreshList();

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;

        SaveData.OnNotesChanged += RefreshList;
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;

        SaveData.OnNotesChanged -= RefreshList;
    }

    private void OnLanguageChanged(Language lang)
    {
        UpdateTexts();
        RefreshList();
    }
}