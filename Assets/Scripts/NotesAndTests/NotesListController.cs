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

    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    private NotesDatabase database;
    private SaveData save;

    private void Start()
    {
        save = SaveSystem.Load();
        LoadDatabase();
        BuildNotesList();
    }

    private void LoadDatabase()
    {
        if (notesJson == null)
        {
            Debug.LogError("[NotesListController] notesJson is not assigned.");
            return;
        }

        database = JsonUtility.FromJson<NotesDatabase>(notesJson.text);

        if (database == null)
        {
            Debug.LogError("[NotesListController] Failed to parse notes database.");
        }
    }

    public void BuildNotesList()
    {
        if (database == null)
        {
            Debug.LogError("[NotesListController] Notes Database is null.");
            return;
        }

        if (save == null)
        {
            Debug.LogError("[NotesListController] SaveData is null.");
            return;
        }

        if (cardsParent == null)
        {
            Debug.LogError("[NotesListController] cardsParent is not assigned.");
            return;
        }

        if (noteCardPrefab == null)
        {
            Debug.LogError("[NotesListController] noteCardPrefab is not assigned.");
            return;
        }

        ClearCards();

        List<NoteData> notes = database.notes;

        if (notes == null || notes.Count == 0)
        {
            Debug.LogWarning("[NotesListController] No notes found.");
            return;
        }

        notes.Sort((a, b) => a.order.CompareTo(b.order));

        foreach (NoteData note in notes)
        {
            if (note == null)
                continue;

            if (showOnlyUnlocked && !IsNoteUnlocked(note))
                continue;

            CreateCard(note);
        }
    }

    // use NoteCardUI prefab to create a card for each note and set up its UI
    private void CreateCard(NoteData note)
    {

        if (noteCardPrefab == null)
        {
            Debug.LogError("Prefab is NULL or destroyed!");
            return;
        }
        GameObject cardObject = Instantiate(noteCardPrefab, cardsParent);
        spawnedCards.Add(cardObject);

        NoteCardUI cardUI = cardObject.GetComponent<NoteCardUI>();

        if (cardUI == null)
        {
            Debug.LogError($"NoteCardUI not found: {note.noteId}");
            return;
        }

        NoteState state = save.GetOrCreateNote(note.noteId);

        cardUI.Setup(note, this, state);
    }

    public void OpenNote(string noteId)
    {
        if (string.IsNullOrEmpty(noteId))
        {
            Debug.LogWarning("[NotesListController] Tried to open note with empty id.");
            return;
        }

        NoteSession.SelectedNoteId = noteId;
        SceneManager.LoadScene(noteDetailSceneName);
    }

    private string GetStatusText(NoteData note)
    {
        NoteState noteState = save.GetOrCreateNote(note.noteId);

        if (IsTestPassed(note.testId))
            return "Тест пройден";

        if (noteState.isRead)
            return "Прочитано";

        return "Новая";
    }

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

    private void ClearCards()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (spawnedCards[i] != null)
                Destroy(spawnedCards[i]);
        }

        spawnedCards.Clear();

        for (int i = cardsParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(cardsParent.GetChild(i).gameObject);
        }
    }

    public void RefreshList()
    {
        BuildNotesList();
    }
}