using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image background;
    [SerializeField] private Image radio;

    [Header("Background Sprites")]
    [SerializeField] private Sprite bgDefault;
    [SerializeField] private Sprite bgSelected;
    [SerializeField] private Sprite bgCorrect;
    [SerializeField] private Sprite bgWrong;

    [Header("Radio Sprites")]
    [SerializeField] private Sprite radioDefault;
    [SerializeField] private Sprite radioSelected;
    [SerializeField] private Sprite radioCorrect;
    [SerializeField] private Sprite radioWrong;

    private AnswerData data;
    private TestController controller;
    private bool isLocked = false;
    private Button button;

    // ================= SETUP =================

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("AnswerButton has no Button component!");
        }
    }

    public void Setup(AnswerData answer, TestController ctrl)
    {
        data = answer;
        controller = ctrl;

        text.text = answer.text;

        background.sprite = bgDefault;
        radio.sprite = radioDefault;

        isLocked = false;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    // ================= CLICK =================

    public void OnClick()
    {
        if (isLocked) return;
        if (data == null)
        {
            Debug.LogError("AnswerData is NULL!");
            return;
        }

        background.sprite = bgSelected;
        radio.sprite = radioSelected;

        Debug.Log($"Clicked: {data.text} | Correct: {data.isCorrect}");

        controller.OnAnswerSelected(this, data);
    }

    // ================= STATES =================

    public void SetCorrect()
    {
        background.sprite = bgCorrect;
        radio.sprite = radioCorrect;
    }

    public void SetWrong()
    {
        background.sprite = bgWrong;
        radio.sprite = radioWrong;
    }

    public bool IsCorrect()
    {
        return data != null && data.isCorrect;
    }

    public void Lock()
    {
        isLocked = true;

        if (button != null)
            button.interactable = false;
    }
}