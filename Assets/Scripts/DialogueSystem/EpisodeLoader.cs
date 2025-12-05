using UnityEngine;
using System.Collections.Generic;

public static class EpisodeLoader
{
    public static EpisodeData LoadEpisode(string episodePath)
    {
        TextAsset asset = Resources.Load<TextAsset>(episodePath);

        if (asset == null)
        {
            Debug.LogError("EpisodeLoader: JSON not found at Resources/" + episodePath);
            return null;
        }

        EpisodeData ep = JsonUtility.FromJson<EpisodeData>(asset.text);
        if (ep == null)
        {
            Debug.LogError("EpisodeLoader: Failed to parse JSON");
            return null;
        }

        // Словарь нод
        ep.nodeDict = new Dictionary<string, DialogueNode>();
        foreach (var n in ep.nodes)
        {
            ep.nodeDict[n.nodeId] = n;
        }

        return ep;
    }
}
