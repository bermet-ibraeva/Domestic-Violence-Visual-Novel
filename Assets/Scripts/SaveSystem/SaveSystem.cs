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

    public static bool RestartCurrentEpisode()
    {
        SaveData save = Load();

        if (save == null)
        {
            Debug.LogError("[SaveSystem] Cannot restart episode: save is null.");
            return false;
        }

        string firstNode = GetEpisodeStartNode(save);

        if (string.IsNullOrEmpty(firstNode))
        {
            Debug.LogError("[SaveSystem] Cannot restart episode: start node not found.");
            return false;
        }

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

        if (save.appliedEffectNodes == null)
            save.appliedEffectNodes = new List<string>();
        else
            save.appliedEffectNodes.Clear();

        Save(save);
        return true;
    }

    private static string GetEpisodeStartNode(SaveData save)
    {
        if (save == null || string.IsNullOrEmpty(save.episodePath))
            return null;

        TextAsset episodeJson = Resources.Load<TextAsset>(save.episodePath);

        if (episodeJson == null)
        {
            Debug.LogError($"[SaveSystem] Episode JSON not found at path: {save.episodePath}");
            return null;
        }

        EpisodeData episode = JsonUtility.FromJson<EpisodeData>(episodeJson.text);

        if (episode == null)
        {
            Debug.LogError("[SaveSystem] Failed to parse EpisodeData from JSON.");
            return null;
        }

        if (episode.scenes == null || episode.scenes.Count == 0)
        {
            Debug.LogError("[SaveSystem] Episode has no scenes.");
            return null;
        }

        if (string.IsNullOrEmpty(episode.scenes[0].startNode))
        {
            Debug.LogError("[SaveSystem] First scene startNode is empty.");
            return null;
        }

        return episode.scenes[0].startNode;
    }
}