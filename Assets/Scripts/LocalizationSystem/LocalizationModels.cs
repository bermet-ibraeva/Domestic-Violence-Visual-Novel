using System;
using System.Collections.Generic;

public enum Language
{
    Russian,
    English,
    Kyrgyz
}

[Serializable]
public class LocalizationItem
{
    public string key;
    public LocalizedEntry value;
}

[Serializable]
public class LocalizedEntry
{
    public string ru;
    public string en;
    public string ky;
}

[Serializable]
public class PageData
{
    public string pageName;
    public List<LocalizationItem> items;

    public bool TryGetEntry(string key, out LocalizedEntry entry)
    {
        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].key == key)
                {
                    entry = items[i].value;
                    return true;
                }
            }
        }

        entry = null;
        return false;
    }
}

[Serializable]
public class LocalizationRoot
{
    public PageData Episode;
    public PageData MainMenu;
    public PageData AboutPage;
    public PageData SettingsPage;
}