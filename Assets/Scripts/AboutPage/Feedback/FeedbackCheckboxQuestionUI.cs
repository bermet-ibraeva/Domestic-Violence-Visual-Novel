using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FeedbackCheckboxQuestionUI : MonoBehaviour, IFeedbackQuestionUI {
    [Header("UI")]
    [SerializeField] private TMP_Text questionText;

    [SerializeField] private Transform optionsParent;

    [Header("Prefabs")]
    [SerializeField] private FeedbackCheckboxOptionUI optionPrefab;

    private FeedbackQuestionData questionData;
    private readonly List<FeedbackCheckboxOptionUI> spawnedOptions = new();

    public void Setup(FeedbackQuestionData question)
    {
        questionData = question;

        questionText.text = L(question.questionKey);

        BuildOptions();
    }

    private void BuildOptions()
    {
        foreach (Transform child in optionsParent)
        {
            Destroy(child.gameObject);
        }

        spawnedOptions.Clear();

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
            L(optionData.textKey));

        spawnedOptions.Add(optionUI);
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

    public void ClearSelection()
    {
        foreach (var option in spawnedOptions)
        {
            option.SetSelected(false);
        }
    }

    private string L(string key)
    {
        return LocalizationManager.Instance
            .GetText("Feedback", key);
    }
}