using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackRadioOptionUI : MonoBehaviour
{
    public enum State
    {
        Default,
        Selected
    }

    [Header("UI")]
    [SerializeField] private TMP_Text text;

    [SerializeField] private Image background;

    [SerializeField] private Image radio;

    [SerializeField] private Button button;

    [Header("Background Sprites")]
    [SerializeField] private Sprite bgDefault;

    [SerializeField] private Sprite bgSelected;

    [Header("Radio Sprites")]
    [SerializeField] private Sprite radioDefault;

    [SerializeField] private Sprite radioSelected;

    private FeedbackOptionData optionData;

    private FeedbackRadioQuestionUI parentQuestion;

    private State currentState = State.Default;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void Setup(
        FeedbackOptionData data,
        FeedbackRadioQuestionUI parent,
        string localizedText)
    {
        optionData = data;

        parentQuestion = parent;

        text.text = localizedText;

        SetState(State.Default);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        parentQuestion.SelectOption(this);
    }

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
        }
    }

    public bool IsSelected()
    {
        return currentState == State.Selected;
    }

    public string GetOptionId()
    {
        return optionData.optionId;
    }
}