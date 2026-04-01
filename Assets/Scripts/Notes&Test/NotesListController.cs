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
    [SerializeField] private string noteDetailSceneName = "NoteDetailScene";

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

    private void CreateCard(NoteData note)
    {
        GameObject card = Instantiate(noteCardPrefab, cardsParent);
        spawnedCards.Add(card);

        Button button = card.GetComponent<Button>();
        if (button == null)
            button = card.GetComponentInChildren<Button>();

        TMP_Text[] texts = card.GetComponentsInChildren<TMP_Text>(true);

        TMP_Text titleText = null;
        TMP_Text previewText = null;
        TMP_Text statusText = null;

        foreach (TMP_Text text in texts)
        {
            string lowerName = text.gameObject.name.ToLower();

            if (titleText == null && lowerName.Contains("title"))
            {
                titleText = text;
                continue;
            }

            if (previewText == null && (lowerName.Contains("preview") || lowerName.Contains("description")))
            {
                previewText = text;
                continue;
            }

            if (statusText == null && lowerName.Contains("status"))
            {
                statusText = text;
                continue;
            }
        }

        if (titleText != null)
            titleText.text = note.title;

        if (previewText != null)
            previewText.text = note.preview;

        if (statusText != null)
            statusText.text = GetStatusText(note);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OpenNote(note.noteId));
        }
        else
        {
            Debug.LogWarning($"[NotesListController] No Button found on card prefab for note: {note.noteId}");
        }
    }

    private void OpenNote(string noteId)
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

        if (IsQuizPassed(note.quizId))
            return "Тест пройден";

        if (noteState.isRead)
            return "Прочитано";

        return "Новая";
    }

    private bool IsNoteUnlocked(NoteData note)
    {
        NoteState noteState = save.GetOrCreateNote(note.noteId);
        return noteState.isUnlocked;
    }

    private bool IsQuizPassed(string quizId)
    {
        if (string.IsNullOrEmpty(quizId))
            return false;

        TestBestScore test = save.GetOrCreateTest(quizId);
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
            Destroy(cardsParent.GetChild(i).gameObject);
        }
    }

    public void RefreshList()
    {
        BuildNotesList();
    }
}