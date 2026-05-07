using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteCardUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text previewText;

    [Header("Footer")]
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private Image actionIcon;

    [Header("Icons")]
    [SerializeField] private Sprite arrowIcon;
    [SerializeField] private Sprite checkIcon;
    [SerializeField] private Sprite lockIcon;

    [Header("UI")]
    [SerializeField] private Button openButton;

    private const int PREVIEW_LIMIT = 80;

    private string noteId;
    private NotesListController controller;

    public void Setup(NoteData data, NotesListController notesController, NoteState state)
    {
        if (data == null)
        {
            Debug.LogError("[NoteCardUI] NoteData is NULL");
            return;
        }

        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("[NoteCardUI] LocalizationManager is NULL");
            return;
        }

        noteId = data.noteId;
        controller = notesController;

        // ================= TEXT =================
        if (titleText != null)
        {
            titleText.text =
                LocalizationManager.Instance.GetText("Notes", data.titleKey);
        }

        if (previewText != null)
        {
            string preview =
                LocalizationManager.Instance.GetText("Notes", data.previewKey);

            previewText.text = TrimPreview(preview);
        }

        // ================= STATE =================
        bool isUnlocked = state != null && state.isUnlocked;
        bool isRead = state != null && state.isRead;

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

        // ================= BUTTON =================
        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OnClickOpen);

            openButton.interactable = isUnlocked;
        }
    }

    // ================= VISUAL STATES =================
    private void SetLockedState()
    {
        if (actionText != null)
        {
            actionText.text =
                LocalizationManager.Instance.GetText("Notes", "note_locked");
        }

        if (actionIcon != null)
        {
            actionIcon.sprite = lockIcon;
        }
    }

    private void SetReadState()
    {
        if (actionText != null)
        {
            actionText.text =
                LocalizationManager.Instance.GetText("Notes", "note_read");
        }

        if (actionIcon != null)
        {
            actionIcon.sprite = checkIcon;
        }
    }

    private void SetNewState()
    {
        if (actionText != null)
        {
            actionText.text =
                LocalizationManager.Instance.GetText("Notes", "note_read_more");
        }

        if (actionIcon != null)
        {
            actionIcon.sprite = arrowIcon;
        }
    }

    // ================= HELPERS =================
    private string TrimPreview(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        if (text.Length <= PREVIEW_LIMIT)
            return text;

        return text.Substring(0, PREVIEW_LIMIT) + "...";
    }

    // ================= EVENTS =================
    private void OnClickOpen()
    {
        if (controller == null)
        {
            Debug.LogWarning("[NoteCardUI] Controller is NULL");
            return;
        }

        SaveData save = SaveManager.Instance.Data;

        if (save == null)
        {
            Debug.LogError("[NoteCardUI] SaveData is NULL");
            return;
        }

        NoteState state = save.GetNote(noteId);

        if (state == null || !state.isUnlocked)
            return;

        controller.OpenNote(noteId);
    }
}