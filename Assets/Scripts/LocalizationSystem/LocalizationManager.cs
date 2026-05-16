using System;
using System.Collections.Generic;
using UnityEngine;

/*
Localization System Architecture

The localization system is responsible for providing multilingual support
throughout the application. Its primary purpose is to separate interface
text and narrative content from program logic, allowing the application
to dynamically switch between supported languages without modifying code.

The system currently supports three languages:

* Russian
* English
* Kyrgyz

General Architecture
The localization architecture is built around several interconnected
components:

* LocalizationManager
* LocalizationRoot
* PageData
* LocalizationItem
* LocalizedEntry
* LanguageSwitcher

Localization Data Structure
All translations are stored in an external JSON file located inside
the Unity Resources/Localization directory. The JSON structure is divided
into pages, where each page represents a separate section of the
application interface or gameplay system.

Examples of pages:

* MainMenu
* Episode
* Notes
* Tests
* Notifications
* SettingsPage

Each page contains multiple localization items identified by unique keys.

Localization hierarchy:
LocalizationRoot
↓
PageData
↓
LocalizationItem
↓
LocalizedEntry

LocalizedEntry stores translated text values for each supported language:

* ru
* en
* ky

This architecture allows the application to retrieve localized text
through a combination of:

* page identifier;
* localization key;
* currently selected language.

LocalizationManager
LocalizationManager acts as the central runtime controller of the
localization subsystem. It is implemented as a persistent Unity singleton
using DontDestroyOnLoad, which allows localization data to remain available
across scene transitions.

Main responsibilities of LocalizationManager:

* loading localization JSON data;
* parsing translation structures;
* storing loaded pages in runtime dictionaries;
* managing the currently selected language;
* saving language preferences;
* providing translated text to interface systems;
* notifying UI components when the language changes.

During application startup, LocalizationManager:

1. Loads the previously selected language from PlayerPrefs;
2. Loads and parses the localization JSON file;
3. Builds runtime dictionaries for fast text retrieval;
4. Marks the localization system as initialized.

Text Retrieval
Localized strings are accessed through:
GetText(pageName, key)

or:
GetText("Page.Key")

The manager first searches for the requested page, then searches for
the corresponding localization key inside that page. After locating the
required entry, the system returns the text corresponding to the currently
active language.

If a translation is missing, the system uses a fallback chain:

1. Russian
2. English
3. Kyrgyz

If no translation exists, a debug placeholder is returned.

Language Switching
Language switching is performed through the LanguageSwitcher component,
which calls:

* SetRussian()
* SetEnglish()
* SetKyrgyz()

These methods forward the selected language to LocalizationManager.

When the active language changes:

* the new value is stored in PlayerPrefs;
* the language selection state is updated;
* the OnLanguageChanged event is triggered.

UI components subscribed to this event automatically refresh their
displayed text without requiring scene reloads.

Runtime Integration
The localization subsystem is integrated into:

* dialogue rendering;
* educational notes;
* test questions and answers;
* notifications;
* menu interfaces;
* statistics displays;
* button labels and descriptions.

Instead of storing direct text inside gameplay data structures,
the application stores localization keys. During runtime,
these keys are resolved into localized strings through
LocalizationManager.

Example flow:
JSON key → LocalizationManager → localized text → UI component

Advantages of the Architecture
The implemented architecture provides several advantages:

* centralized localization management;
* separation of content and program logic;
* simplified addition of new languages;
* dynamic language switching during runtime;
* reduced duplication of interface text;
* improved maintainability of narrative content.

Overall, the localization subsystem functions as a centralized multilingual
content management layer integrated across all major gameplay and interface
systems within the application.
*/

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
                    Debug.LogError("[Localization] LocalizationManager not found in scene!");
                    return null;
                }
            }

            return instance;
        }
    }

    [Header("JSON file in Resources/Localization")]
    [SerializeField] private string jsonFileName = "localizationData";

    public Language CurrentLanguage { get; private set; } = Language.Russian;
    public bool HasSelectedLanguage { get; private set; }
    public bool IsLoaded { get; private set; }

    public event Action<Language> OnLanguageChanged;

    private const string LanguagePrefKey = "game_language";
    private const string LanguageSelectedPrefKey = "game_language_selected";

    private LocalizationRoot localizationData;

    private readonly Dictionary<string, PageData> pages = new();

    private void Awake()
    {
        Debug.Log("[Localization] Awake");

        if (instance != null && instance != this)
        {
            Debug.Log("[Localization] Duplicate destroyed");

            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        LoadSavedLanguage();
        LoadLocalizationJson();

        Debug.Log($"[Localization] Loaded pages count: {pages.Count}");
    }

    // =========================
    // LANGUAGE
    // =========================

    private void LoadSavedLanguage()
    {
        HasSelectedLanguage =
            PlayerPrefs.GetInt(LanguageSelectedPrefKey, 0) == 1;

        if (!PlayerPrefs.HasKey(LanguagePrefKey))
        {
            CurrentLanguage = Language.Russian;
            return;
        }

        string saved =
            PlayerPrefs.GetString(
                LanguagePrefKey,
                Language.Russian.ToString()
            );

        if (Enum.TryParse(saved, out Language parsedLanguage))
        {
            CurrentLanguage = parsedLanguage;
        }
        else
        {
            CurrentLanguage = Language.Russian;
        }
    }

    public void SetLanguage(Language language)
    {
        // IMPORTANT FIX:
        // Allows selecting Russian on first launch.
        if (CurrentLanguage == language && HasSelectedLanguage)
            return;

        CurrentLanguage = language;

        HasSelectedLanguage = true;

        PlayerPrefs.SetString(
            LanguagePrefKey,
            CurrentLanguage.ToString()
        );

        PlayerPrefs.SetInt(
            LanguageSelectedPrefKey,
            1
        );

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

    // =========================
    // GET TEXT
    // =========================

    public string GetText(string pageName, string key)
    {
        if (string.IsNullOrWhiteSpace(pageName) ||
            string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

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

        // Split only first dot
        string[] parts = fullKey.Split('.', 2);

        if (parts.Length != 2)
        {
            Debug.LogWarning(
                $"[Localization] Invalid key format: {fullKey}. Use Page.Key"
            );

            return $"#{fullKey}";
        }

        return GetText(parts[0], parts[1]);
    }

    private string GetLocalizedValue(
        LocalizedEntry entry,
        string pageName,
        string key
    )
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

        // fallback chain
        if (!string.IsNullOrEmpty(entry.ru))
            return entry.ru;

        if (!string.IsNullOrEmpty(entry.en))
            return entry.en;

        if (!string.IsNullOrEmpty(entry.ky))
            return entry.ky;

        Debug.LogWarning(
            $"[Localization] Empty translations for key: {pageName}.{key}"
        );

        return $"#{pageName}.{key}";
    }

    // =========================
    // LOAD JSON
    // =========================

    private void LoadLocalizationJson()
    {
        pages.Clear();

        TextAsset jsonFile =
            Resources.Load<TextAsset>(
                $"Localization/{jsonFileName}"
            );

        if (jsonFile == null)
        {
            Debug.LogError(
                $"[Localization] JSON file not found at Resources/Localization/{jsonFileName}"
            );

            return;
        }

        localizationData =
            JsonUtility.FromJson<LocalizationRoot>(
                jsonFile.text
            );

        if (localizationData == null)
        {
            Debug.LogError(
                "[Localization] Failed to parse localization JSON."
            );

            return;
        }

        AddPage(localizationData.MainMenu);
        AddPage(localizationData.Episode);
        AddPage(localizationData.AboutPage);
        AddPage(localizationData.SettingsPage);
        AddPage(localizationData.Stats);
        AddPage(localizationData.Notifications);
        AddPage(localizationData.Notes);
        AddPage(localizationData.Tests);
        AddPage(localizationData.Feedback);

        Debug.Log($"[Localization] Loaded pages: {pages.Count}");

        IsLoaded = true;
    }

    private void AddPage(PageData page)
    {
        if (page == null)
            return;

        if (string.IsNullOrWhiteSpace(page.pageName))
            return;

        pages[page.pageName] = page;
    }
}