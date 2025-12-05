using UnityEngine;
using System.Collections.Generic;

public static class EpisodeLoader
{
    public static EpisodeData LoadEpisode(string episodePath, out Dictionary<string, DialogueNode> nodeDict)
    {
        nodeDict = null;

        // Загружаем JSON из Resources
        TextAsset asset = Resources.Load<TextAsset>(episodePath);

        if (asset == null)
        {
            Debug.LogError("EpisodeLoader: JSON not found at Resources/" + episodePath);
            return null;
        }

        // Парсим JSON
        EpisodeData episode = JsonUtility.FromJson<EpisodeData>(asset.text);

        if (episode == null)
        {
            Debug.LogError("EpisodeLoader: Failed to parse JSON");
            return null;
        }

        // Создаем словарь нод
        nodeDict = new Dictionary<string, DialogueNode>();
        foreach (DialogueNode node in episode.nodes)
        {
            if (!nodeDict.ContainsKey(node.nodeId))
                nodeDict.Add(node.nodeId, node);
        }

        return episode;
    }
}
