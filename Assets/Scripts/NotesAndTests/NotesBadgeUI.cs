using TMPro;
using UnityEngine;

public class NotesBadgeUI : MonoBehaviour
{
    [SerializeField] private GameObject badgeObject;
    [SerializeField] private TMP_Text badgeText;

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        SaveData.OnNotesChanged += Refresh;
        Refresh();
    }


    private void OnDisable()
    {
        SaveData.OnNotesChanged -= Refresh;
    }

    public void Refresh()
    {
        if (SaveManager.Instance == null ||
            SaveManager.Instance.Data == null)
        {
            SetCount(0);
            return;
        }

        int unreadCount =
            SaveManager.Instance.Data.GetUnreadNotesCount();

        SetCount(unreadCount);
    }

    private void SetCount(int count)
    {
        bool show = count > 0;

        if (badgeObject != null)
            badgeObject.SetActive(show);

        if (show && badgeText != null)
            badgeText.text = count.ToString();
    }
}