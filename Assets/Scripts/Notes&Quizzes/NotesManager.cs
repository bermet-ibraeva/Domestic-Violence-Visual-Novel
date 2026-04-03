using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NotesManager : MonoBehaviour
{
    [Header("Data")]
    public TextAsset notesJson;

    [Header("UI")]
    public Transform cardsRoot;
    public NoteCardUI cardPrefab;

    [Header("Scene")]
    public string noteDetailSceneName = "NoteDetail";

    private SaveData save;
    private NotesDatabase database;

    private void Start()
    {
        save = SaveSystem.Load();
        LoadDatabase();
        RefreshList();
    }

    private void LoadDatabase()
    {
        if (notesJson == null)
        {
            Debug.LogError("[NotesManager] notesJson is missing");
            return;
        }

        database = JsonUtility.FromJson<NotesDatabase>(notesJson.text);

        if (database == null)
            database = new NotesDatabase();
    }

    public void RefreshList()
    {
        ClearCards();

        if (database == null || database.notes == null)
        {
            Debug.LogError("[NotesManager] Database is null");
            return;
        }

        List<NoteData> unlockedNotes = GetUnlockedNotes();

        foreach (var note in unlockedNotes)
        {
            NoteCardUI card = Instantiate(cardPrefab, cardsRoot);
            card.Setup(note, this);
        }
    }

    private List<NoteData> GetUnlockedNotes()
    {
        List<NoteData> result = new List<NoteData>();

        foreach (var note in database.notes)
        {
            NoteState state = save.GetOrCreateNote(note.noteId);

            if (state.isUnlocked)
                result.Add(note);
        }

        result.Sort((a, b) => a.order.CompareTo(b.order));
        return result;
    }

    private void ClearCards()
    {
        for (int i = cardsRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(cardsRoot.GetChild(i).gameObject);
        }
    }

    public void OpenNote(string noteId)
    {
        if (string.IsNullOrEmpty(noteId))
        {
            Debug.LogWarning("[NotesManager] OpenNote called with empty noteId");
            return;
        }

        NoteState noteState = save.GetOrCreateNote(noteId);

        if (!noteState.isRead)
        {
            noteState.isRead = true;

            if (!noteState.rewardClaimed)
            {
                noteState.rewardClaimed = true;
                save.sparksTotal += 2;
                save.episodeSparks += 2;
            }

            SaveSystem.Save(save);
        }

        NoteSession.SelectedNoteId = noteId;
        SceneManager.LoadScene(noteDetailSceneName);
    }
}