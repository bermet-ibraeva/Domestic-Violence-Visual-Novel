using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteCardUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public Button openButton;

    private string noteId;
    private NotesManager manager;

    public void Setup(NoteData data, NotesManager notesManager)
    {
        noteId = data.noteId;
        manager = notesManager;

        if (titleText != null)
            titleText.text = data.title;

        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OnClickOpen);
        }
    }

    private void OnClickOpen()
    {
        if (manager != null)
            manager.OpenNote(noteId);
    }
}