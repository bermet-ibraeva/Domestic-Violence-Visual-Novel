using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("CSV file in Resources/Localization")]
    [SerializeField] private string csvFileName = "dialogue_localization";

    public Language CurrentLanguage { get; private set; } = Language.Russian;

    private readonly Dictionary<string, LocalizedText> localizedTexts = new();

    public event Action<Language> OnLanguageChanged;

    private const string LanguagePrefKey = "game_language";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSavedLanguage();
        LoadLocalizationTable();
    }

    private void LoadSavedLanguage()
    {
        if (PlayerPrefs.HasKey(LanguagePrefKey))
        {
            CurrentLanguage = (Language)PlayerPrefs.GetInt(LanguagePrefKey);
        }
        else
        {
            CurrentLanguage = Language.Russian;
        }
    }

    public void SetLanguage(Language language)
    {
        if (CurrentLanguage == language)
            return;

        CurrentLanguage = language;
        PlayerPrefs.SetInt(LanguagePrefKey, (int)CurrentLanguage);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke(CurrentLanguage);
    }

    public string GetText(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        if (!localizedTexts.TryGetValue(key, out var entry))
        {
            Debug.LogWarning($"[Localization] Key not found: {key}");
            return $"#{key}";
        }

        return CurrentLanguage switch
        {
            Language.Russian => string.IsNullOrEmpty(entry.ru) ? Fallback(entry, key) : entry.ru,
            Language.English => string.IsNullOrEmpty(entry.en) ? Fallback(entry, key) : entry.en,
            Language.Kyrgyz  => string.IsNullOrEmpty(entry.ky) ? Fallback(entry, key) : entry.ky,
            _ => Fallback(entry, key)
        };
    }

    private string Fallback(LocalizedText entry, string key)
    {
        if (!string.IsNullOrEmpty(entry.ru)) return entry.ru;
        if (!string.IsNullOrEmpty(entry.en)) return entry.en;
        if (!string.IsNullOrEmpty(entry.ky)) return entry.ky;

        Debug.LogWarning($"[Localization] Empty translations for key: {key}");
        return $"#{key}";
    }

    private void LoadLocalizationTable()
    {
        localizedTexts.Clear();

        TextAsset csvFile = Resources.Load<TextAsset>($"Localization/{csvFileName}");
        if (csvFile == null)
        {
            Debug.LogError($"[Localization] CSV file not found at Resources/Localization/{csvFileName}");
            return;
        }

        List<string[]> rows = ParseCsv(csvFile.text);

        if (rows.Count <= 1)
        {
            Debug.LogWarning("[Localization] CSV is empty or has no data rows.");
            return;
        }

        // expected header: key,ru,en,ky
        for (int i = 1; i < rows.Count; i++)
        {
            string[] row = rows[i];

            if (row.Length < 4)
            {
                Debug.LogWarning($"[Localization] Row {i + 1} has not enough columns.");
                continue;
            }

            string key = row[0].Trim();
            if (string.IsNullOrEmpty(key))
                continue;

            localizedTexts[key] = new LocalizedText
            {
                ru = row[1],
                en = row[2],
                ky = row[3]
            };
        }

        Debug.Log($"[Localization] Loaded {localizedTexts.Count} entries.");
    }

    private List<string[]> ParseCsv(string csvText)
    {
        var result = new List<string[]>();
        var rows = csvText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row))
                continue;

            result.Add(ParseCsvLine(row));
        }

        return result;
    }

    private string[] ParseCsvLine(string line)
    {
        List<string> fields = new();
        bool inQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        fields.Add(current);
        return fields.ToArray();
    }

    [Serializable]
    private class LocalizedText
    {
        public string ru;
        public string en;
        public string ky;
    }
}