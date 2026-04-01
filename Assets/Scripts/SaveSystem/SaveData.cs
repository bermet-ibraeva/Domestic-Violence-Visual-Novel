using System;
using System.Collections.Generic;


// score saved by player
[Serializable]
public class SaveData
{
    public string episodePath = "Episodes/episode_1";
    public string currentNodeId = "E01_S01_start";
    public int chapterNumber = 1;

    public int sparksTotal;
    public bool episodeRewardGranted;

    public int trustAG;
    public int trustJA;

    public int riskTotal;
    public int safetyTotal;

    public int episodeRisk;
    public int episodeSafety;
    public int episodeTrustAG;
    public int episodeTrustJA;
    public int episodeSparks;

    public List<string> completedEpisodes = new List<string>();

    public EpisodeSnapshot episodeStartSnapshot;
    public List<string> appliedEffectNodes = new List<string>();

    public List<string> shownNotificationIds = new List<string>();

    public List<NoteState> notes = new List<NoteState>();
    public List<TestBestScore> testsBest = new List<TestBestScore>();

    // Получить или создать заметку
    public NoteState GetOrCreateNote(string noteId)
    {
        NoteState note = notes.Find(n => n.noteId == noteId);

        if (note == null)
        {
            note = new NoteState
            {
                noteId = noteId,
                isUnlocked = false,
                isRead = false,
                rewardClaimed = false
            };

            notes.Add(note);
        }

        return note;
    }

    //  Получить или создать тест
    public TestBestScore GetOrCreateTest(string quizId)
    {
        TestBestScore test = testsBest.Find(t => t.quizId == quizId);

        if (test == null)
        {
            test = new TestBestScore
            {
                quizId = quizId,
                bestScore = 0
            };

            testsBest.Add(test);
        }

        return test;
    }
}

[Serializable]
public class NoteState
{
    public string noteId;
    public bool isUnlocked;
    public bool isRead;
    public bool rewardClaimed;
}

[Serializable]
public class TestBestScore
{
    public string quizId;
    public int bestScore; // 0–3
}