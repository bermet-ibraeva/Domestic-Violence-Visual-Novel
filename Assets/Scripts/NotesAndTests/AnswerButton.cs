using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerButton : MonoBehaviour
{
    public enum State
    {
        Default,
        Selected,
        Correct,
        Wrong,
        Disabled
    }

    [Header("UI")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image background;
    [SerializeField] private Image radio;
    [SerializeField] private Button button;

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

    private State currentState = State.Default;


    // ================= INIT =================
    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
            Debug.LogError("AnswerButton has no Button component!");
    }

    public void Setup(AnswerData answer, TestController ctrl)
    {
        data = answer;
        controller = ctrl;

        RefreshText();

        SetState(State.Default);

        button.interactable = true;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }


    // ================= STATE =================
    public void SetState(State state)
    {
        currentState = state;
        ApplyState();
    }

    private void ApplyState()
    {
        switch (currentState)
        {
            case State.Default:
                background.sprite = bgDefault;
                radio.sprite = radioDefault;
                break;

            case State.Selected:
                background.sprite = bgSelected;
                radio.sprite = radioSelected;
                break;

            case State.Correct:
                background.sprite = bgCorrect;
                radio.sprite = radioCorrect;
                break;

            case State.Wrong:
                background.sprite = bgWrong;
                radio.sprite = radioWrong;
                break;

            case State.Disabled:
                background.sprite = bgDefault;
                radio.sprite = radioDefault;
                break;
        }
    }


    // ================= LOCALIZATION =================
    public void RefreshText()
    {
        if (data == null || string.IsNullOrEmpty(data.textKey))
            return;

        text.text = LocalizationManager.Instance.GetText("Tests", data.textKey);

        GetComponent<SimpleButtonResize>()?.RefreshSize();
    }

    // ================= INTERACTION =================
    private void OnClick()
    {
        if (!button.interactable || data == null)
            return;

        SetState(State.Selected);

#if UNITY_EDITOR
                Debug.Log($"Clicked answer: {data.textKey} | Correct: {data.isCorrect}");
#endif

        controller.OnAnswerSelected(this, data);
    }


    // ================= HELPERS =================
    public bool IsCorrect()
    {
        return data != null && data.isCorrect;
    }

    public void Lock()
    {
        button.interactable = false;
        SetState(State.Disabled);
    }

    public void MarkCorrect()
    {
        SetState(State.Correct);
    }

    public void MarkWrong()
    {
        SetState(State.Wrong);
    }
}