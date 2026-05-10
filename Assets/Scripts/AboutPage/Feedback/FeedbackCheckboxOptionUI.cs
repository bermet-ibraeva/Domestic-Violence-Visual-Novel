using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackCheckboxOptionUI : MonoBehaviour
{
    public enum State
    {
        Inactive,
        Active
    }

    [Header("UI")]
    [SerializeField] private TMP_Text text;

    [SerializeField] private Image background;

    [SerializeField] private Image checkbox;

    [SerializeField] private Button button;

    [Header("Background Sprites")]
    [SerializeField] private Sprite bgInactive;

    [SerializeField] private Sprite bgActive;

    [Header("Checkbox Sprites")]
    [SerializeField] private Sprite checkboxInactive;

    [SerializeField] private Sprite checkboxActive;

    private FeedbackOptionData optionData;

    private State currentState = State.Inactive;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    public void Setup(
        FeedbackOptionData data,
        string localizedText)
    {
        optionData = data;

        text.text = localizedText;

        SetState(State.Inactive);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (currentState == State.Inactive)
        {
            SetState(State.Active);
        }
        else
        {
            SetState(State.Inactive);
        }
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
            case State.Inactive:
                background.sprite = bgInactive;
                checkbox.sprite = checkboxInactive;
                break;

            case State.Active:
                background.sprite = bgActive;
                checkbox.sprite = checkboxActive;
                break;
        }
    }

    public bool IsSelected()
    {
        return currentState == State.Active;
    }

    public string GetOptionId()
    {
        return optionData.optionId;
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            SetState(State.Active);
        }
        else
        {
            SetState(State.Inactive);
        }
    }
}