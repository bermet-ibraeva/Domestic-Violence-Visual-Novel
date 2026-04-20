using System;
using System.Collections.Generic;


// containes all the data that needs to be saved and loaded
[Serializable]
public class SaveData
{
    // Player Progress
    public string episodePath;
    public string currentNodeId;

    // Player Total Stats
    public int sparksTotal;
    public int trustAGTotal;
    public int trustJATotal;
    public int riskTotal;
    public int safetyTotal;
    
    // Flags and Service Data
    public EpisodeSnapshot episodeStartSnapshot; // super important: used for episode restart and for calculating rewards at the end of episode
    public bool episodeRewardGranted;
    public List<string> completedEpisodes = new List<string>();

    public List<string> appliedEffectNodes = new List<string>();
    public List<string> shownNotificationIds = new List<string>();

    // Learning Progress
    public List<NoteState> notes = new List<NoteState>();
    public List<TestBestScore> testsBest = new List<TestBestScore>();

    public void ResetEpisodeState()
    {
        appliedEffectNodes.Clear();
        shownNotificationIds.Clear();
        episodeRewardGranted = false;
    }

    // helper methods to get or create note and test data
    public NoteState GetOrCreateNote(string noteId)
    {
        if (notes == null)
            notes = new List<NoteState>();

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

    // for tests we only track the best score, so we can easily calculate rewards and show progress in the UI
    public TestBestScore GetOrCreateTest(string testId)
    {
        if (testsBest == null)
            testsBest = new List<TestBestScore>();

        TestBestScore test = testsBest.Find(t => t.testId == testId);

        if (test == null)
        {
            test = new TestBestScore
            {
                testId = testId,
                bestScore = 0
            };

            testsBest.Add(test);
        }

        return test;
    }
}