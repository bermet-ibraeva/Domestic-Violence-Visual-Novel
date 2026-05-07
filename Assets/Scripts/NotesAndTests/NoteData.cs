using System;
using System.Collections.Generic;


[Serializable]
public class NoteData
{
    public string noteId;
    public string titleKey;
    public string previewKey; /* maybe add later: check how it will look first */
    public string image;
    public string textKey;
    public string testId;
    public int order;
}

[Serializable]
public class NotesDatabase
{
    public List<NoteData> notes = new List<NoteData>();

    public NoteData GetNoteById(string noteId)
    {
        if (string.IsNullOrEmpty(noteId))
            return null;

        return notes.Find(n => n.noteId == noteId);
    }
}

public static class NoteSession
{
    public static string SelectedNoteId;
}


[Serializable]
public class TestData
{
    public string testId;
    public string noteId;
    public string titleKey;
    public int maxReward;
    public int questionsPerRun;
    public List<QuestionData> questions;
}

[Serializable]
public class TestDatabase
{
    public List<TestData> tests = new List<TestData>();

    public TestData GetTestById(string testId)
    {
        if (string.IsNullOrEmpty(testId))
            return null;

        return tests.Find(q => q.testId == testId);
    }
}

public static class TestSession
{
    public static string SelectedTestId;
}


[Serializable]
public class QuestionData
{
    public string questionId;
    public string questionKey;
    public List<AnswerData> answers;
}

[Serializable]
public class AnswerData
{
    public string answerId;
    public string textKey;
    public bool isCorrect;
}


[System.Serializable]
public class NoteState
{
    public string noteId;
    public bool isUnlocked;
    public bool isRead;
    public bool readRewardClaimed;

}

[System.Serializable]
public class TestBestScore
{
    public string testId;

    public int bestScore;

    public int claimedReward;
}