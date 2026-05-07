using System;

[Serializable]
public class EpisodeSnapshot
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
}