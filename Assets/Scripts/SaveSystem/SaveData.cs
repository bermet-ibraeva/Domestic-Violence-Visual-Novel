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

    public List<string> notesRewarded = new List<string>();
    public List<TestBestScore> testsBest = new List<TestBestScore>();

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
}