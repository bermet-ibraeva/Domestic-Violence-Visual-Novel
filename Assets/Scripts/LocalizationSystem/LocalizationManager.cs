using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager instance;

    public static LocalizationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LocalizationManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("LocalizationManager");
                    instance = obj.AddComponent<LocalizationManager>();

                    Debug.Log("[Localization] Auto-created LocalizationManager");
                }
            }

            return instance;
        }
    }

    [Header("JSON file in Resources/Localization")]
    [SerializeField] private string jsonFileName = "localizationData";

    public Language CurrentLanguage { get; private set; } = Language.Russian;
    public bool HasSelectedLanguage { get; private set; }

    public event Action<Language> OnLanguageChanged;

    private const string LanguagePrefKey = "game_language";
    private const string LanguageSelectedPrefKey = "game_language_selected";

    private LocalizationRoot localizationData;
    private readonly Dictionary<string, PageData> pages = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSavedLanguage();
        LoadLocalizationJson();
    }

    // ================= LANGUAGE =================

    private void LoadSavedLanguage()
    {
        HasSelectedLanguage = PlayerPrefs.GetInt(LanguageSelectedPrefKey, 0) == 1;

        if (!PlayerPrefs.HasKey(LanguagePrefKey))
        {
            CurrentLanguage = Language.Russian;
            return;
        }

        string saved = PlayerPrefs.GetString(LanguagePrefKey, Language.Russian.ToString());

        if (Enum.TryParse(saved, out Language parsedLanguage))
            CurrentLanguage = parsedLanguage;
        else
            CurrentLanguage = Language.Russian;
    }

    public void SetLanguage(Language language)
    {
        if (CurrentLanguage == language)
            return;

        CurrentLanguage = language;
        HasSelectedLanguage = true;

        PlayerPrefs.SetString(LanguagePrefKey, CurrentLanguage.ToString());
        PlayerPrefs.SetInt(LanguageSelectedPrefKey, 1);
        PlayerPrefs.Save();

        Debug.Log($"[Localization] Language set to: {CurrentLanguage}");

        OnLanguageChanged?.Invoke(CurrentLanguage);
    }

    public void SetDefaultLanguage()
    {
        if (HasSelectedLanguage)
            return;

        SetLanguage(Language.Russian);

        Debug.Log("[Localization] Default language applied.");
    }

    public bool IsFirstLaunch()
    {
        return !HasSelectedLanguage;
    }

    // ================= GET TEXT =================

    public string GetText(string pageName, string key)
    {
        if (string.IsNullOrWhiteSpace(pageName) || string.IsNullOrWhiteSpace(key))
            return string.Empty;

        if (!pages.TryGetValue(pageName, out PageData page))
        {
            Debug.LogWarning($"[Localization] Page not found: {pageName}");
            return $"#{pageName}.{key}";
        }

        if (!page.TryGetEntry(key, out LocalizedEntry entry))
        {
            Debug.LogWarning($"[Localization] Key not found: {pageName}.{key}");
            return $"#{pageName}.{key}";
        }

        return GetLocalizedValue(entry, pageName, key);
    }

    public string GetText(string fullKey)
    {
        if (string.IsNullOrWhiteSpace(fullKey))
            return string.Empty;

        string[] parts = fullKey.Split('.');

        if (parts.Length != 2)
        {
            Debug.LogWarning($"[Localization] Invalid key format: {fullKey}. Use Page.Key");
            return $"#{fullKey}";
        }

        return GetText(parts[0], parts[1]);
    }

    private string GetLocalizedValue(LocalizedEntry entry, string pageName, string key)
    {
        string value = CurrentLanguage switch
        {
            Language.Russian => entry.ru,
            Language.English => entry.en,
            Language.Kyrgyz => entry.ky,
            _ => entry.ru
        };

        if (!string.IsNullOrEmpty(value))
            return value;

        if (!string.IsNullOrEmpty(entry.ru)) return entry.ru;
        if (!string.IsNullOrEmpty(entry.en)) return entry.en;
        if (!string.IsNullOrEmpty(entry.ky)) return entry.ky;

        Debug.LogWarning($"[Localization] Empty translations for key: {pageName}.{key}");
        return $"#{pageName}.{key}";
    }

    // ================= LOAD JSON =================

    private void LoadLocalizationJson()
    {
        pages.Clear();

        TextAsset jsonFile = Resources.Load<TextAsset>($"Localization/{jsonFileName}");
        if (jsonFile == null)
        {
            Debug.LogError($"[Localization] JSON file not found at Resources/Localization/{jsonFileName}");
            return;
        }

        localizationData = JsonUtility.FromJson<LocalizationRoot>(jsonFile.text);

        if (localizationData == null)
        {
            Debug.LogError("[Localization] Failed to parse localization JSON.");
            return;
        }

        AddPage(localizationData.MainMenu);
        AddPage(localizationData.Episode);
        AddPage(localizationData.AboutPage);
        AddPage(localizationData.SettingsPage);

        Debug.Log($"[Localization] Loaded pages: {pages.Count}");
    }

    private void AddPage(PageData page)
    {
        if (page == null || string.IsNullOrWhiteSpace(page.pageName))
            return;

        pages[page.pageName] = page;
    }
}