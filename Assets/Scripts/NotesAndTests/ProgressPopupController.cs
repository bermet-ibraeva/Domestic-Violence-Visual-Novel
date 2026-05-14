using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressPopupController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TextAsset notesJson;

    [SerializeField] private TextAsset testsJson;

    [Header("Popup")]
    [SerializeField] private GameObject popupRoot;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private TMP_Text notesTitleText;
    [SerializeField] private TMP_Text notesProgressText;

    [SerializeField] private TMP_Text testsTitleText;
    [SerializeField] private TMP_Text testsProgressText;

    [SerializeField] private TMP_Text closeButtonText;

    [Header("Progress Bars")]
    [SerializeField] private Image notesFill;

    [SerializeField] private Image testsFill;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    private NotesDatabase notesDatabase;
    private TestDatabase testsDatabase;

    // ================= UNITY =================

    private void Awake()
    {
        LoadDatabases();

        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged +=
                HandleLanguageChanged;
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -=
                HandleLanguageChanged;
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePopup);
        }
    }

    // ================= DATABASE =================

    private void LoadDatabases()
    {
        if (notesJson != null)
        {
            notesDatabase =
                JsonUtility.FromJson<NotesDatabase>(notesJson.text);
        }

        if (testsJson != null)
        {
            testsDatabase =
                JsonUtility.FromJson<TestDatabase>(testsJson.text);
        }

        if (notesDatabase == null)
        {
            Debug.LogError("[ProgressPopup] NotesDatabase is NULL");
        }

        if (testsDatabase == null)
        {
            Debug.LogError("[ProgressPopup] TestsDatabase is NULL");
        }
    }

    // ================= POPUP =================

    public void OpenPopup()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(true);
        }

        RefreshUI();
    }

    public void ClosePopup()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
    }

    // ================= UI =================

    private void RefreshUI()
    {
        if (SaveManager.Instance == null ||
            SaveManager.Instance.Data == null)
        {
            Debug.LogError("[ProgressPopup] SaveData is NULL");
            return;
        }

        SaveData data = SaveManager.Instance.Data;

        // ================= NOTES =================

        int totalNotes = 0;
        int readNotes = 0;

        if (data.notes != null)
        {
            foreach (var note in data.notes)
            {
                if (note == null || !note.isUnlocked)
                    continue;

                totalNotes++;

                if (note.isRead)
                {
                    readNotes++;
                }
            }
        }

        // ================= TESTS =================

        int totalRewards = 0;
        int earnedRewards = 0;

        if (testsDatabase != null)
        {
            foreach (var test in testsDatabase.tests)
            {
                if (test == null)
                    continue;

                NoteState noteState =
                    data.GetNote(test.noteId);

                bool unlocked =
                    noteState != null &&
                    noteState.isUnlocked;

                if (!unlocked)
                    continue;

                totalRewards += test.maxReward;
            }
        }

        if (data.testsBest != null)
        {
            foreach (var test in data.testsBest)
            {
                if (test != null)
                {
                    earnedRewards += test.claimedReward;
                }
            }
        }

        // ================= TEXTS =================

        if (titleText != null)
        {
            titleText.text = L("progress_title");
        }

        if (notesTitleText != null)
        {
            notesTitleText.text = L("notes_page_title");
        }

        if (testsTitleText != null)
        {
            testsTitleText.text = L("tests_title");
        }

        if (notesProgressText != null)
        {
            notesProgressText.text =
                L("notes_progress")
                .Replace("{0}", readNotes.ToString())
                .Replace("{1}", totalNotes.ToString());
        }

        if (testsProgressText != null)
        {
            testsProgressText.text =
                L("tests_progress")
                .Replace("{0}", earnedRewards.ToString())
                .Replace("{1}", totalRewards.ToString());
        }

        if (closeButtonText != null)
        {
            closeButtonText.text = L("close");
        }

        // ================= FILLS =================

        if (notesFill != null)
        {
            notesFill.fillAmount =
                totalNotes > 0
                ? (float)readNotes / totalNotes
                : 0f;
        }

        if (testsFill != null)
        {
            testsFill.fillAmount =
                totalRewards > 0
                ? (float)earnedRewards / totalRewards
                : 0f;
        }
    }

    // ================= LOCALIZATION =================

    private void HandleLanguageChanged(Language language)
    {
        RefreshUI();
    }

    private string L(string key)
    {
        return LocalizationManager.Instance
            .GetText("Notes", key);
    }
}