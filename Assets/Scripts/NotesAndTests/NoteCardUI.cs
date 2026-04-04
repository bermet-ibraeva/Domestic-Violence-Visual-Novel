using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button openButton;

    private string noteId;
    private NotesListController controller;

    public void Setup(NoteData data, NotesListController notesController, string status)
    {
        noteId = data.noteId;
        controller = notesController;

        if (titleText != null)
            titleText.text = data.title;

        if (previewText != null)
            previewText.text = data.preview;

        if (statusText != null)
            statusText.text = status;

        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OnClickOpen);
        }
    }

    private void OnClickOpen()
    {
        if (controller != null)
            controller.OpenNote(noteId);
    }
}