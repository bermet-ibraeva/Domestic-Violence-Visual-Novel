using System;

[Serializable]
public class TestBestScore
{
    public string testId;
    public int bestScore;
}

[Serializable]
public class EpisodeSnapshot
{
    public string episodePath;
    public string nodeId;
    public int trustAG;
    public int trustJA;
    public int riskTotal;
    public int safetyTotal;
    public int episodeRisk;
    public int episodeSafety;
    public int sparksTotal;
}
