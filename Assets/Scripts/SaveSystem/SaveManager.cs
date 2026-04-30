using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

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
        DontDestroyOnLoad(gameObject);

        Init();
    }

    // ================= INIT =================

    private void Init()
    {
        Load();

        if (Data == null)
        {
            Debug.LogWarning($"{TAG} Save was null → creating new");
            Data = SaveSystem.CreateNew();
        }
    }

    // ================= CORE =================

    public void Load()
    {
        Data = SaveManager.Instance.Data;

        if (Data == null)
        {
            Debug.LogWarning($"{TAG} Load returned null");
        }
    }

    public void Save()
    {
        if (Data == null)
        {
            Debug.LogError($"{TAG} Cannot save → Data is null");
            return;
        }

        SaveSystem.Save(Data);
    }

    public void Clear()
    {
        SaveSystem.Clear();
        Data = SaveSystem.CreateNew();
    }

    // ================= EPISODE =================
    public void StartEpisode(string episodePath)
    {
        SaveSystem.StartEpisode(episodePath);
        Load();
    }

    public bool RestartEpisode()
    {
        bool result = SaveSystem.RestartCurrentEpisode();

        if (result)
            Load(); 

        return result;
    }
}