using System;
using System.Collections.Generic;


[Serializable]
public class NoteData
{
    public string noteId;
    public string title;
    public string preview; /* maybe add later: check how it will look first */
    public string text;
    public string quizId;
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
public class QuizData
{
    public string quizId;
    public string noteId;
    public string title;
    public int maxReward;
    public int questionsPerRun;
    public List<QuestionData> questions;
}

[Serializable]
public class QuizDatabase
{
    public List<QuizData> quizzes = new List<QuizData>();

    public QuizData GetQuizById(string quizId)
    {
        if (string.IsNullOrEmpty(quizId))
            return null;

        return quizzes.Find(q => q.quizId == quizId);
    }
}

public static class QuizSession
{
    public static string SelectedQuizId;
}


[Serializable]
public class QuestionData
{
    public string questionId;
    public string questionText;
    public List<AnswerData> answers;
}

[Serializable]
public class AnswerData
{
    public string answerId;
    public string text;
    public bool isCorrect;
}


