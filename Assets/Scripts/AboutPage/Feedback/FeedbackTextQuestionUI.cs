using TMPro;
using UnityEngine;

public class FeedbackTextQuestionUI :
    MonoBehaviour,
    IFeedbackQuestionUI
{
    [Header("UI")]
    [SerializeField] private TMP_Text questionText;

    [SerializeField] private TMP_InputField inputField;

    [Header("Settings")]
    [SerializeField] private int defaultMaxCharacters = 500;
    [SerializeField] private TMP_Text placeholderText;

    private FeedbackQuestionData questionData;

    public void Setup(FeedbackQuestionData question)
    {
        questionData = question;

        questionText.text = L(question.questionKey);

        if (placeholderText != null && !string.IsNullOrEmpty(question.placeholderKey)) {
            placeholderText.text =
                L(question.placeholderKey);
        }
        ApplySettings();
    }

    private void ApplySettings()
    {
        int maxCharacters =
            questionData.maxCharacters > 0
            ? questionData.maxCharacters
            : defaultMaxCharacters;

        inputField.characterLimit = maxCharacters;
    }

    public FeedbackAnswer GetAnswer()
    {
        FeedbackAnswer answer = new FeedbackAnswer
        {
            questionId = questionData.questionId,

            textAnswer =
                inputField.text.Trim()
        };

        return answer;
    }

    public bool IsValid()
    {
        if (!questionData.required)
            return true;

        string text =
            inputField.text.Trim();

        return !string.IsNullOrEmpty(text);
    }

    private string L(string key)
    {
        return LocalizationManager.Instance
            .GetText("Feedback", key);
    }
}