using System;

[Serializable]
public class EpisodeSnapshot // temp value for current episode stats, will be reset on episode start
{
    public int sparks;
    public int trustAG;
    public int trustJA;
    public int risk;
    public int safety;
}

public static class TempGameContext
{
    public static EpisodeSnapshot CurrentEpisode = new EpisodeSnapshot();

    public static void ResetEpisode()
    {
        CurrentEpisode = new EpisodeSnapshot();
    }

    public static SaveData saveToLoad;
}