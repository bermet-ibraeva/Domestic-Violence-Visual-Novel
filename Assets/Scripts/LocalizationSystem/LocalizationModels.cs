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
}

[Serializable]
public class LocalizationRoot
{
    public PageData MainMenu;
    public PageData AboutPage;
    public PageData SettingsPage;
}