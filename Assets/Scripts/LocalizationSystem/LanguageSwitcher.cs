using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    public void SetRussian()
    {
        LocalizationManager.Instance.SetLanguage(Language.Russian);
    }

    public void SetEnglish()
    {
        LocalizationManager.Instance.SetLanguage(Language.English);
    }

    public void SetKyrgyz()
    {
        LocalizationManager.Instance.SetLanguage(Language.Kyrgyz);
    }
}