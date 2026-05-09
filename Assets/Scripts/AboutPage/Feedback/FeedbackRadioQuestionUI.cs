using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FeedbackRadioQuestionUI :
    MonoBehaviour,
    IFeedbackQuestionUI
{
    [Header("UI")]
    [SerializeField] private TMP_Text questionText;

    [SerializeField] private Transform optionsParent;

    [Header("Prefabs")]
    [SerializeField] private FeedbackRadioOptionUI optionPrefab;

    private FeedbackQuestionData questionData;

    private readonly List<FeedbackRadioOptionUI>
        spawnedOptions = new();

    public void Setup(FeedbackQuestionData question)
    {
        questionData = question;

        questionText.text = L(question.questionKey);

        BuildOptions();
    }

    private void BuildOptions()
    {
        foreach (var option in questionData.options)
        {
            CreateOption(option);
        }
    }

    private void CreateOption(
        FeedbackOptionData optionData)
    {
        var optionUI =
            Instantiate(optionPrefab, optionsParent);

        optionUI.Setup(
            optionData,
            this,
            L(optionData.textKey));

        spawnedOptions.Add(optionUI);
    }

    public void SelectOption(
        FeedbackRadioOptionUI selectedOption)
    {
        foreach (var option in spawnedOptions)
        {
            option.SetState(
                FeedbackRadioOptionUI.State.Default);
        }

        selectedOption.SetState(
            FeedbackRadioOptionUI.State.Selected);
    }

    public FeedbackAnswer GetAnswer()
    {
        FeedbackAnswer answer = new FeedbackAnswer
        {
            questionId = questionData.questionId
        };

        foreach (var option in spawnedOptions)
        {
            if (option.IsSelected())
            {
                answer.selectedOptionIds
                    .Add(option.GetOptionId());

                break;
            }
        }

        return answer;
    }

    public bool IsValid()
    {
        if (!questionData.required)
            return true;

        foreach (var option in spawnedOptions)
        {
            if (option.IsSelected())
            {
                return true;
            }
        }

        return false;
    }

    private string L(string key)
    {
        return LocalizationManager.Instance
            .GetText("Feedback", key);
    }
}