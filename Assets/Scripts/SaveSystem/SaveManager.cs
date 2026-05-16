using UnityEngine;

/*
Save Management System

The save system is responsible for storing, restoring, and managing
persistent player progression throughout the application. The architecture
is divided into three main layers: SaveData, SaveSystem, and SaveManager.

1. SaveData
   SaveData functions as the primary data container that stores all persistent
   game information. This includes:

* current episode and dialogue position;
* accumulated gameplay statistics;
* unlocked educational content;
* completed tests and best scores;
* notification states;
* episode progression flags.

The class also contains helper methods for managing notes, rewards,
statistics, and runtime progression updates. In addition, event callbacks
are used to notify interface components when important values such as
sparks or notes are changed.

2. SaveSystem
   SaveSystem provides low-level serialization and storage functionality.
   Its responsibilities include:

* converting SaveData into JSON format;
* restoring SaveData from stored JSON;
* saving data into Unity PlayerPrefs;
* creating new save files;
* clearing existing save data;
* normalizing collections after loading.

The SaveSystem does not manage gameplay logic directly. Instead, it acts
as a utility layer responsible for persistent data storage and retrieval.

3. SaveManager
   SaveManager acts as the central runtime controller of the save architecture.
   It is implemented as a persistent Unity singleton using DontDestroyOnLoad,
   allowing save data to remain accessible across scene transitions.

Main responsibilities of SaveManager:

* loading save data during application startup;
* providing global access to the current SaveData instance;
* triggering save and autosave operations;
* managing episode initialization and restart logic;
* restoring player progression;
* resetting temporary runtime data;
* coordinating interaction between gameplay systems and persistent storage.

When the application starts, SaveManager attempts to load an existing save
through SaveSystem. If no save exists, a new save structure is created
automatically.

During gameplay, DialogueController and other systems interact directly
with SaveManager to:

* update progression state;
* save current dialogue position;
* modify gameplay statistics;
* unlock notes and rewards;
* restore episode progress after restarting the application.

Episode Runtime Logic
At the beginning of an episode, SaveManager creates a snapshot of the
current cumulative statistics. This snapshot is later used for:

* restarting episodes without losing global progression;
* calculating episode-specific changes;
* generating final episode summaries.

When an episode is restarted, the system restores statistics from the
saved snapshot, clears temporary runtime states, and returns the player
to the episode starting node.

Overall Architecture Flow:
Gameplay Systems
↓
SaveManager
↓
SaveData
↓
SaveSystem
↓
PlayerPrefs Storage

This layered structure separates gameplay progression logic from low-level
storage operations, improving maintainability, scalability, and reliability
of the save architecture.
*/


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public SaveData Data { get; private set; }

    private const string TAG = "[SaveManager]";

    private void Awake()
    {
        Debug.Log($"{TAG} Awake | Scene: {gameObject.scene.name}");

        if (Instance != null && Instance != this)
        {
            Debug.Log($"{TAG} Duplicate destroyed");

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


    private void OnDestroy()
    {
        Debug.Log($"{TAG} Destroyed | Scene: {gameObject.scene.name}");

        if (Instance == this)
        {
            Instance = null;
        }
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

        SaveData.NotifySparksChanged();
        SaveData.NotifyNotesChanged();
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
        SaveData.NotifySparksChanged();
        SaveData.NotifyNotesChanged();

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
        SaveData.NotifySparksChanged();
        SaveData.NotifyNotesChanged();

        TempGameContext.ResetEpisode();

        Debug.Log($"{TAG} Save cleared");
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

        Data.episodePath = episodePath;

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
            Data.SetSparks(Data.episodeStartSnapshot.sparks);
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