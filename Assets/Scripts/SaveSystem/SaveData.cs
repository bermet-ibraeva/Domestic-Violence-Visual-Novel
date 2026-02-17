using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // 🔹 Progress tracking
    public string episodePath;
    public string currentNodeId;
    public int chapterNumber;

    // Spark system (global)
    public int sparksTotal;

    // TODO:Spark reward history for notes (check if note was already rewarded)
    public List<string> notesRewarded = new List<string>();

    // TODO: Best score for tests history (if less then 5, then can be added, if 5, then only if better than the worst score)
    public List<TestBestScore> testsBest = new List<TestBestScore>();

    // Trust system (global across episodes)
    public int trustAG;   // Ainaz ↔ Guldana
    public int trustJA;   // Jamila ↔ Aida

    // Global accumulated safety & risk (optional but useful)
    public int riskTotal;
    public int safetyTotal;

    // 🔹 Current episode local values
    public int episodeRisk;
    public int episodeSafety;

    // 🔹 Episodes already completed (prevents spark farming)
    public List<string> completedEpisodes = new List<string>();

    // 🔹 Snapshot for restart logic
    public EpisodeSnapshot episodeStartSnapshot;
    public List<string> appliedEffectNodes = new List<string>();

}
