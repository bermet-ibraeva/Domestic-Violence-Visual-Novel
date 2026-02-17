public static class EpisodeLoader
{
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
        if (episode.nodes != null)
        {
            foreach (var node in episode.nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.nodeId)) continue;

                if (!nodeDict.ContainsKey(node.nodeId))
                    nodeDict.Add(node.nodeId, node);
                else
                    Debug.LogWarning($"EpisodeLoader: Duplicate nodeId '{node.nodeId}' in {episodePath}");
            }
        }

        return episode;
    }
}
