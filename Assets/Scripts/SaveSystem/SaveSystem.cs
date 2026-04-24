using UnityEngine;
using System;
using System.Collections.Generic;


// mechanism of save and load : how to save data
public static class SaveSystem
{
    private const string KEY = "VN_SAVE"; // main personal save of player -> PlayPrefs

    public static void Save(SaveData data)
    {
        if (data == null)
            return;

        Normalize(data);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(KEY);
    }

    public static SaveData Load()
    {
        if (!HasSave())
            return CreateNew();

        string json = PlayerPrefs.GetString(KEY);

        if (string.IsNullOrEmpty(json))
            return CreateNew();

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
            return CreateNew();

        Normalize(data);
        return data;
    }

    public static SaveData CreateNew()
    {
        SaveData data = new SaveData();
        Normalize(data);
        Save(data);
        return data;
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
    }

    private static void Normalize(SaveData data)
    {
        if (data.completedEpisodes == null)
            data.completedEpisodes = new List<string>();

        if (data.appliedEffectNodes == null)
            data.appliedEffectNodes = new List<string>();

        if (data.shownNotificationIds == null)
            data.shownNotificationIds = new List<string>();

        if (data.notes == null)
            data.notes = new List<NoteState>();

        if (data.testsBest == null)
            data.testsBest = new List<TestBestScore>();
    }

    public static void StartEpisode(string episodePath)
    {
        SaveData save = SaveSystem.Load();

        if (save.shownNotificationIds != null)
            save.shownNotificationIds.Clear();

        save.episodePath = episodePath;

        string startNode = GetEpisodeStartNode(episodePath);

        if (string.IsNullOrEmpty(startNode))
        {
            Debug.LogError("[SaveSystem] Start node not found.");
            return;
        }

        save.currentNodeId = startNode;

        save.episodeStartSnapshot = new EpisodeSnapshot
        {
            sparks = save.sparksTotal,
            trustAG = save.trustAGTotal,
            trustJA = save.trustJATotal,
            risk = save.riskTotal,
            safety = save.safetyTotal
        };

        SaveSystem.Save(save);
        Debug.Log($"[SaveSystem] Starting episode: {episodePath}");
    }

    private static string GetEpisodeStartNode(string episodePath)
    {
        TextAsset episodeJson = Resources.Load<TextAsset>(episodePath);

        if (episodeJson == null)
        {
            Debug.LogError($"Episode not found: {episodePath}");
            return null;
        }

        EpisodeData episode = JsonUtility.FromJson<EpisodeData>(episodeJson.text);

        if (episode == null || episode.scenes == null || episode.scenes.Count == 0)
            return null;

        foreach (var scene in episode.scenes)
        {
            if (!string.IsNullOrEmpty(scene.startNode))
                return scene.startNode;
        }

        return null;
    }

    public static bool RestartCurrentEpisode()
    {
        SaveData save = Load();

        if (save == null)
        {
            Debug.LogError("[SaveSystem] Cannot restart episode: save is null.");
            return false;
        }

        if (save.shownNotificationIds != null)
            save.shownNotificationIds.Clear();

        if (string.IsNullOrEmpty(save.episodePath))
        {
            Debug.LogError("[SaveSystem] episodePath is empty.");
            return false;
        }

        string firstNode = GetEpisodeStartNode(save.episodePath);

        if (string.IsNullOrEmpty(firstNode))
        {
            Debug.LogError("[SaveSystem] Cannot restart episode: start node not found.");
            return false;
        }

        // вернуть snapshot
        if (save.episodeStartSnapshot != null)
        {
            save.sparksTotal = save.episodeStartSnapshot.sparks;
            save.trustAGTotal = save.episodeStartSnapshot.trustAG;
            save.trustJATotal = save.episodeStartSnapshot.trustJA;
            save.riskTotal = save.episodeStartSnapshot.risk;
            save.safetyTotal = save.episodeStartSnapshot.safety;
        }

        save.episodeRewardGranted = false;
        save.currentNodeId = firstNode;

        // очистка эффектов
        if (save.appliedEffectNodes == null)
            save.appliedEffectNodes = new List<string>();
        else
            save.appliedEffectNodes.Clear();

        Save(save);
        return true;
    }   
}