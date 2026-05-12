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

    [Header("Progress")]
    [SerializeField] private Button progressButton;
    [SerializeField] private ProgressPopupController progressPopup;


    private readonly List<GameObject> spawnedCards = new();

    private NotesDatabase database;

    // ================= UNITY =================
    private void Start()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
        {
            Debug.LogError("[NotesList] SaveManager is NULL");
            return;
        }

        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("[NotesList] LocalizationManager is NULL");
            return;
        }

        if (progressButton != null)
        {
            progressButton.onClick.AddListener(OpenProgressPopup);
        }

        LoadDatabase();
        UpdateTexts();
        BuildNotesList();
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }

        SaveData.OnNotesChanged += RefreshList;

        if (database != null)
        {
            RefreshList();
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }

        if (progressButton != null)
        {
            progressButton.onClick.RemoveListener(OpenProgressPopup);
        }

        SaveData.OnNotesChanged -= RefreshList;
    }

    // ================= DATABASE =================
    private void LoadDatabase()
    {
        if (notesJson == null)
        {
            Debug.LogError("[NotesList] notesJson is NULL");
            return;
        }

        database = JsonUtility.FromJson<NotesDatabase>(notesJson.text);

        if (database == null)
        {
            Debug.LogError("[NotesList] Failed to parse notes database");
        }
    }

    // ================= BUILD =================
    public void BuildNotesList()
    {
        if (database == null)
        {
            Debug.LogError("[NotesList] Database is NULL");
            return;
        }

        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
        {
            Debug.LogError("[NotesList] SaveManager is NULL");
            return;
        }

        if (cardsParent == null)
        {
            Debug.LogError("[NotesList] cardsParent is NULL");
            return;
        }

        if (noteCardPrefab == null)
        {
            Debug.LogError("[NotesList] noteCardPrefab is NULL");
            return;
        }

        ClearCards();

        List<NoteData> notes = database.notes;

        if (notes == null || notes.Count == 0)
        {
            Debug.LogWarning("[NotesList] No notes found");
            return;
        }

        notes.Sort((a, b) => a.order.CompareTo(b.order));

        bool hasUnlockedNotes = false;

        foreach (NoteData note in notes)
        {
            if (note == null)
                continue;

            bool isUnlocked = IsNoteUnlocked(note.noteId);

            if (showOnlyUnlocked && !isUnlocked)
                continue;

            CreateCard(note);

            hasUnlockedNotes = true;
        }

        // fallback:
        // if no unlocked notes -> show locked notes too
        if (!hasUnlockedNotes)
        {
            foreach (NoteData note in notes)
            {
                if (note == null)
                    continue;

                CreateCard(note);
            }
        }
    }

    private void CreateCard(NoteData note)
    {
        if (note == null)
            return;

        GameObject cardObject =
            Instantiate(noteCardPrefab, cardsParent);

        spawnedCards.Add(cardObject);

        NoteCardUI cardUI =
            cardObject.GetComponent<NoteCardUI>();

        if (cardUI == null)
        {
            Debug.LogError(
                "[NotesList] NoteCardUI missing on prefab"
            );

            Destroy(cardObject);

            return;
        }

        NoteState state = SaveManager.Instance.Data.GetOrCreateNote(note.noteId);

        cardUI.Setup(note, this, state);
    }

    // ================= LOGIC =================
    private bool IsNoteUnlocked(string noteId)
    {
        if (string.IsNullOrEmpty(noteId))
            return false;

        NoteState state = SaveManager.Instance.Data.GetNote(noteId);

        return state != null && state.isUnlocked;
    }

    // ================= NAVIGATION =================
    public void OpenNote(string noteId)
    {
        if (string.IsNullOrEmpty(noteId))
        {
            Debug.LogWarning("[NotesList] noteId is empty");
            return;
        }

        NoteState state = SaveManager.Instance.Data.GetNote(noteId);

        if (state == null || !state.isUnlocked)
        {
            Debug.LogWarning(
                "[NotesList] Tried to open locked note: " + noteId
            );

            return;
        }

        NoteSession.SelectedNoteId = noteId;

        SceneManager.LoadScene(noteDetailSceneName);
    }

    // ================= UI =================
    private void ClearCards()
    {
        if (cardsParent == null)
            return;

        for (int i = cardsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(cardsParent.GetChild(i).gameObject);
        }

        spawnedCards.Clear();
    }

    public void RefreshList()
    {
        if (database == null)
            return;

        BuildNotesList();
    }

    private void UpdateTexts()
    {
        if (LocalizationManager.Instance == null)
            return;

        if (pageTitleText != null)
        {
            pageTitleText.text =
                LocalizationManager.Instance.GetText(
                    "Notes",
                    "notes_page_title"
                );
        }

        if (introText != null)
        {
            introText.text =
                LocalizationManager.Instance.GetText(
                    "Notes",
                    "notes_intro"
                );
        }
    }

    private void OpenProgressPopup()
    {
        if (progressPopup != null)
        {
            progressPopup.OpenPopup();
        }
    }

    // ================= LOCALIZATION =================
    private void OnLanguageChanged(Language lang)
    {
        UpdateTexts();
        RefreshList();
    }
}