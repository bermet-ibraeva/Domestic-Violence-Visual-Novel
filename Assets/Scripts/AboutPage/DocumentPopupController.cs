using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocumentPopupController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject popupRoot;

    [SerializeField] private TMP_Text titleText;

    [SerializeField] private TMP_Text contentText;

    [SerializeField] private TMP_Text readButtonText;

    [SerializeField] private Button readButton;

    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }

        if (readButton != null)
        {
            readButton.onClick.AddListener(ClosePopup);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    public void OpenPopup(
        string title,
        string content,
        string readText)
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (contentText != null)
        {
            contentText.text = TextFormatter.Format(content);
        }

        if (readButtonText != null)
        {
            readButtonText.text = readText;
        }
    }

    public void ClosePopup()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (readButton != null)
        {
            readButton.onClick.RemoveListener(ClosePopup);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePopup);
        }
    }
}