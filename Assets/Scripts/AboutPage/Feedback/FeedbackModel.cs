using System;
using System.Collections.Generic;

[Serializable]
public class FeedbackFormData
{
    public string formId;
    public string titleKey;

    public List<FeedbackQuestionData> questions;
}

[Serializable]
public class FeedbackDatabase
{
    public List<FeedbackFormData> forms = new();

    public FeedbackFormData GetFormById(string formId)
    {
        return forms.Find(f => f.formId == formId);
    }
}

public enum FeedbackQuestionType
{
    Radio,
    Checkbox,
    Text
}

[Serializable]
public class FeedbackQuestionData
{
    public string questionId;

    public string questionKey;

    public FeedbackQuestionType questionType;

    public bool required;
    
    public string placeholderKey;

    public int maxCharacters = 500;

    public List<FeedbackOptionData> options;
}

[Serializable]
public class FeedbackOptionData
{
    public string optionId;

    public string textKey;
}

[Serializable]
public class FeedbackResponse
{
    public string formId;

    public string timestamp;

    public string language;

    public string platform;

    public string sessionId;

    public List<FeedbackAnswer> answers = new();
}

[Serializable]
public class FeedbackAnswer
{
    public string questionId;

    public List<string> selectedOptionIds = new();

    public string textAnswer;
}