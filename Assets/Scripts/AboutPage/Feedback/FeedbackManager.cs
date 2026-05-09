using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TextAsset feedbackJson;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private Transform questionsParent;

    [Header("Prefabs")]
    [SerializeField] private FeedbackRadioQuestionUI radioPrefab;
    [SerializeField] private FeedbackCheckboxQuestionUI checkboxPrefab;
    [SerializeField] private FeedbackTextQuestionUI textPrefab;

    [Header("Buttons")]
    [SerializeField] private Button submitButton;

    [Header("Optional")]
    [SerializeField] private string formId = "main_feedback";
    [SerializeField] private FeedbackService feedbackService;

    private FeedbackDatabase database;
    private FeedbackFormData currentForm;

    private readonly List<IFeedbackQuestionUI> spawnedQuestions = new();

    private void Start()
    {
        LoadDatabase();
        BuildForm();

        submitButton.onClick.AddListener(SubmitFeedback);
    }

    private void LoadDatabase()
    {
        if (feedbackJson == null)
        {
            Debug.LogError("[Feedback] Feedback JSON is missing.");
            return;
        }

        database = JsonUtility.FromJson<FeedbackDatabase>(feedbackJson.text);

        if (database == null)
        {
            Debug.LogError("[Feedback] Failed to parse feedback database.");
            return;
        }

        currentForm = database.GetFormById(formId);

        if (currentForm == null)
        {
            Debug.LogError($"[Feedback] Form not found: {formId}");
            return;
        }
    }

    private void BuildForm()
    {
        if (currentForm == null)
            return;

        titleText.text = L(currentForm.titleKey);

        foreach (var question in currentForm.questions)
        {
            CreateQuestion(question);
        }

        submitButton.transform.SetAsLastSibling();
    }


    private void CreateQuestion(FeedbackQuestionData question)
    {
        switch (question.questionType)
        {
            case FeedbackQuestionType.Radio:
                CreateRadioQuestion(question);
                break;

            case FeedbackQuestionType.Checkbox:
                CreateCheckboxQuestion(question);
                break;

            case FeedbackQuestionType.Text:
                CreateTextQuestion(question);
                break;
        }
    }

    private void CreateRadioQuestion(FeedbackQuestionData question)
    {
        var ui = Instantiate(radioPrefab, questionsParent);

        ui.Setup(question);

        spawnedQuestions.Add(ui);
    }

    private void CreateCheckboxQuestion(FeedbackQuestionData question)
    {
        var ui = Instantiate(checkboxPrefab, questionsParent);

        ui.Setup(question);

        spawnedQuestions.Add(ui);
    }

    private void CreateTextQuestion(FeedbackQuestionData question)
    {
        var ui = Instantiate(textPrefab, questionsParent);

        ui.Setup(question);

        spawnedQuestions.Add(ui);
    }

    private void SubmitFeedback()
    {
        if (!ValidateForm())
        {
            return;
        }

        FeedbackResponse response = new FeedbackResponse {
            formId = currentForm.formId,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            language = LocalizationManager.Instance.CurrentLanguage.ToString(),
            platform = Application.platform.ToString(),
            sessionId = Guid.NewGuid().ToString()
        };

        foreach (var questionUI in spawnedQuestions){
            var answer = questionUI.GetAnswer();

            if (answer != null)
            {
                response.answers.Add(answer);
            }
        }

        SaveFeedback(response);
        submitButton.interactable = false;
        StartCoroutine(feedbackService.SendFeedback(response, OnFeedbackSent));
        Debug.Log("[Feedback] Feedback submitted.");
    }

    private void OnFeedbackSent(bool success)
    {
        if (success)
        {
            Debug.Log("[Feedback] Upload complete.");

            submitButton.interactable = false;

            // TODO:
            // Show success popup
        }
        else
        {
            Debug.LogError("[Feedback] Upload failed.");
            submitButton.interactable = true;

            // TODO:
            // Show error popup
        }
    }

    private void SaveFeedback(FeedbackResponse response)
    {
        string json = JsonUtility.ToJson(response, true);

        string fileName =
            $"feedback_{DateTime.Now:yyyyMMdd_HHmmss}.json";

        string path =
            System.IO.Path.Combine(Application.persistentDataPath, fileName);

        System.IO.File.WriteAllText(path, json);

        Debug.Log($"[Feedback] Saved to: {path}");
    }

    private string L(string key)
    {
        return LocalizationManager.Instance.GetText("Feedback", key);
    }

    private bool ValidateForm()
    {
        foreach (var questionUI in spawnedQuestions)
        {
            if (!questionUI.IsValid())
            {
                Debug.LogWarning(
                    "[Feedback] Validation failed.");

                return false;
            }
        }

        return true;
    }

}