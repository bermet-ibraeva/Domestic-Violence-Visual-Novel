using UnityEngine;
using System.Collections.Generic;

public static class EpisodeLoader
{
    /// <summary>
    /// episodePath = путь в Resources БЕЗ .json
    /// Например: "Episodes/episode_1" -> Resources/Episodes/episode_1.json
    /// </summary>
    public static EpisodeData LoadEpisode(string episodePath, out Dictionary<string, DialogueNode> nodeDict)
    {
        nodeDict = null;

        TextAsset asset = Resources.Load<TextAsset>(episodePath);

        if (asset == null)
        {
            Debug.LogError("EpisodeLoader: JSON not found at Resources/" + episodePath);
            return null;
        }

        EpisodeData episode = JsonUtility.FromJson<EpisodeData>(asset.text);

        if (episode == null)
        {
            Debug.LogError("EpisodeLoader: Failed to parse JSON");
            return null;
        }

        nodeDict = new Dictionary<string, DialogueNode>();
        foreach (var node in episode.nodes)
        {
            if (!nodeDict.ContainsKey(node.nodeId))
                nodeDict.Add(node.nodeId, node);
        }

        return episode;
    }
}
