using System.Collections.Generic;
using UnityEngine;

/*
EpisodeLoader

This static class loads episode JSON data from the Resources folder
and builds fast lookup dictionaries for nodes and scenes.

Responsibilities:
- Loads EpisodeData from a JSON file in Resources.
- Parses the JSON into EpisodeData object using JsonUtility.
- Builds the following dictionaries for fast access:
    • nodeDict: nodeId -> DialogueNode
    • sceneDict: sceneId -> SceneData
    • nodeToScene: nodeId -> SceneData (maps which scene each node belongs to)
- Handles error cases:
    • empty episodePath
    • JSON file not found
    • JSON parsing errors
    • missing or duplicate sceneIds or nodeIds
- Skips null scenes or nodes gracefully
- Logs warnings/errors to help debug episode structure

Usage:
- Call LoadEpisode with the path to episode JSON.
- Get the episode object and the dictionaries for runtime dialogue access.
*/

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
            Debug.LogError($"Episode not found in Resources: '{episodePath}'");
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
            return null;
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
                else
                {
                    Debug.LogWarning($"[EpisodeLoader] Node '{node.nodeId}' already mapped to a scene.");
                }
            }
        }

        return episode;
    }
}
