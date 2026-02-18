using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loads EpisodeData JSON from Resources and builds fast lookup dictionaries:
/// - nodeDict: nodeId -> DialogueNode
/// - sceneDict: sceneId -> SceneData
/// - nodeToScene: nodeId -> SceneData (which scene this node belongs to)
///
/// episodePath example: "Episodes/episode_1" -> Resources/Episodes/episode_1.json
/// </summary>
public static class EpisodeLoader
{
    public static EpisodeData LoadEpisode(
        string episodePath,
        out Dictionary<string, DialogueNode> nodeDict,
        out Dictionary<string, SceneData> sceneDict,
        out Dictionary<string, SceneData> nodeToScene
    )
    {
        nodeDict = new Dictionary<string, DialogueNode>();
        sceneDict = new Dictionary<string, SceneData>();
        nodeToScene = new Dictionary<string, SceneData>();

        if (string.IsNullOrEmpty(episodePath))
        {
            Debug.LogError("[EpisodeLoader] episodePath is empty.");
            return null;
        }

        TextAsset asset = Resources.Load<TextAsset>(episodePath);
        if (asset == null)
        {
            Debug.LogError($"[EpisodeLoader] Episode json not found in Resources: '{episodePath}.json'");
            return null;
        }

        EpisodeData episode;
        try
        {
            episode = JsonUtility.FromJson<EpisodeData>(asset.text);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EpisodeLoader] Failed to parse JSON at '{episodePath}': {ex.Message}");
            return null;
        }

        if (episode == null)
        {
            Debug.LogError($"[EpisodeLoader] Parsed episode is null: '{episodePath}'");
            return null;
        }

        if (episode.scenes == null || episode.scenes.Count == 0)
        {
            Debug.LogError($"[EpisodeLoader] Episode has no scenes: '{episodePath}'");
            return episode;
        }

        // Build dictionaries
        foreach (var scene in episode.scenes)
        {
            if (scene == null)
                continue;

            if (string.IsNullOrEmpty(scene.sceneId))
            {
                Debug.LogWarning("[EpisodeLoader] Scene has empty sceneId. Skipping.");
                continue;
            }

            if (!sceneDict.ContainsKey(scene.sceneId))
                sceneDict.Add(scene.sceneId, scene);
            else
                Debug.LogWarning($"[EpisodeLoader] Duplicate sceneId '{scene.sceneId}'");

            if (scene.nodes == null)
                continue;

            foreach (var node in scene.nodes)
            {
                if (node == null)
                    continue;

                if (string.IsNullOrEmpty(node.nodeId))
                {
                    Debug.LogWarning($"[EpisodeLoader] Node with empty nodeId in scene '{scene.sceneId}'. Skipping.");
                    continue;
                }

                if (!nodeDict.ContainsKey(node.nodeId))
                {
                    nodeDict.Add(node.nodeId, node);
                }
                else
                {
                    Debug.LogWarning($"[EpisodeLoader] Duplicate nodeId '{node.nodeId}' (scene '{scene.sceneId}').");
                }

                // Map node -> its scene (first wins)
                if (!nodeToScene.ContainsKey(node.nodeId))
                    nodeToScene.Add(node.nodeId, scene);
            }
        }

        return episode;
    }
}
