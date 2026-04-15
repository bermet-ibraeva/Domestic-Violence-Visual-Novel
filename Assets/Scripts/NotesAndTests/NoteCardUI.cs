using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteCardUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text previewText;

    [Header("Footer")]
    [SerializeField] private TMP_Text actionText; // "Читать далее" / "Прочитано"
    [SerializeField] private Image actionIcon;

    [Header("Icons")]
    [SerializeField] private Sprite arrowIcon;
    [SerializeField] private Sprite checkIcon;
    [SerializeField] private Sprite lockIcon;

    [Header("UI")]
    [SerializeField] private Button openButton;
    private string noteId;
    private NotesListController controller;

    public void Setup(NoteData data, NotesListController notesController, NoteState state)
    {
        noteId = data.noteId;
        controller = notesController;

        //  TEXT
        if (titleText != null)
            titleText.text = data.title;

        if (previewText != null)
            previewText.text = data.preview;

        //  STATE LOGIC
            bool isUnlocked = state.isUnlocked;
        bool isRead = state.isRead;

        if (!isUnlocked)
        {
            SetLockedState();
        }
        else if (isRead)
        {
            SetReadState();
        }
        else
        {
            SetNewState();
        }

        // 🔘 BUTTON
        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OnClickOpen);
            openButton.interactable = true; // TODO: later change it to openButton.interactable = isUnlocked; if you want to disable button for locked notes
        }
    }

    private void SetLockedState()
    {
        if (actionText != null)
            actionText.text = "Не доступно";

        if (actionIcon != null)
            actionIcon.sprite = lockIcon;
    }

    private void SetReadState()
    {
        if (actionText != null)
            actionText.text = "Прочитано";

        if (actionIcon != null)
            actionIcon.sprite = checkIcon;
    }

    private void SetNewState()
    {
        if (actionText != null)
            actionText.text = "Читать далее";

        if (actionIcon != null)
            actionIcon.sprite = arrowIcon;
    }

    private string TrimPreview(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text.Length > 80 ? text.Substring(0, 80) + "..." : text;
    }

    private void OnClickOpen()
    {
        if (controller != null)
            controller.OpenNote(noteId);
    }
}