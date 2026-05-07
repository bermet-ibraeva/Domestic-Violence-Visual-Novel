using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public SaveData Data { get; private set; }

    private const string TAG = "[SaveManager]";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    // ================= INIT =================

    private void Initialize()
    {
        Data = SaveSystem.Load();

        if (Data == null)
        {
            Debug.Log($"{TAG} Creating new save");

            Data = SaveSystem.CreateNew();
        }
    }

    // ================= SAVE CORE =================

    public void Save()
    {
        if (Data == null)
        {
            Debug.LogError($"{TAG} Cannot save -> Data is NULL");
            return;
        }

        SaveSystem.Save(Data);
    }

    public void AutoSave()
    {
        Save();
    }

    public void Reload()
    {
        Data = SaveSystem.Load();

        if (Data == null)
        {
            Debug.LogWarning($"{TAG} Reload failed -> creating new save");

            Data = SaveSystem.CreateNew();
        }
    }

    public void Clear()
    {
        SaveSystem.Clear();

        Data = SaveSystem.CreateNew();

        TempGameContext.CurrentEpisode = null;

        Debug.Log($"{TAG} Save cleared");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // ================= EPISODE RUNTIME =================

    private void ResetRuntimeEpisode()
    {
        TempGameContext.CurrentEpisode = new EpisodeSnapshot
        {
            sparks = 0,
            trustAG = 0,
            trustJA = 0,
            risk = 0,
            safety = 0
        };
    }

    // ================= EPISODES =================

    public void StartEpisode(string episodePath)
    {
        if (Data == null)
        {
            Debug.LogError($"{TAG} Data is NULL");
            return;
        }

        Data.ResetEpisodeState();

        ResetRuntimeEpisode();

        Data.episodePath = episodePath;

        string startNode = GetEpisodeStartNode(episodePath);

        if (string.IsNullOrEmpty(startNode))
        {
            Debug.LogError($"{TAG} Start node not found");
            return;
        }

        Data.currentNodeId = startNode;

        Data.episodeStartSnapshot = new EpisodeSnapshot
        {
            sparks = Data.sparksTotal,
            trustAG = Data.trustAGTotal,
            trustJA = Data.trustJATotal,
            risk = Data.riskTotal,
            safety = Data.safetyTotal
        };

        Save();

        Debug.Log($"{TAG} Started episode: {episodePath}");
    }

    public bool RestartEpisode()
    {
        if (Data == null)
        {
            Debug.LogError($"{TAG} Data is NULL");
            return false;
        }

        if (string.IsNullOrEmpty(Data.episodePath))
        {
            Debug.LogError($"{TAG} episodePath is empty");
            return false;
        }

        string startNode = GetEpisodeStartNode(Data.episodePath);

        if (string.IsNullOrEmpty(startNode))
        {
            Debug.LogError($"{TAG} Start node not found");
            return false;
        }

        // rollback totals to snapshot
        if (Data.episodeStartSnapshot != null)
        {
            Data.sparksTotal = Data.episodeStartSnapshot.sparks;
            Data.trustAGTotal = Data.episodeStartSnapshot.trustAG;
            Data.trustJATotal = Data.episodeStartSnapshot.trustJA;
            Data.riskTotal = Data.episodeStartSnapshot.risk;
            Data.safetyTotal = Data.episodeStartSnapshot.safety;
        }

        Data.ResetEpisodeState();

        ResetRuntimeEpisode();

        Data.currentNodeId = startNode;

        Save();

        Debug.Log($"{TAG} Episode restarted");

        return true;
    }

    // ================= HELPERS =================

    private string GetEpisodeStartNode(string episodePath)
    {
        TextAsset episodeJson = Resources.Load<TextAsset>(episodePath);

        if (episodeJson == null)
        {
            Debug.LogError($"{TAG} Episode not found: {episodePath}");
            return null;
        }

        EpisodeData episode = JsonUtility.FromJson<EpisodeData>(episodeJson.text);

        if (episode == null || episode.scenes == null)
            return null;

        foreach (var scene in episode.scenes)
        {
            if (!string.IsNullOrEmpty(scene.startNode))
                return scene.startNode;
        }

        return null;
    }
}