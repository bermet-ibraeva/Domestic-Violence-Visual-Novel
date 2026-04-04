using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedTextUI : MonoBehaviour
{
    [SerializeField] private string key;

    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        UpdateText();
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(Language lang)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        textComponent.text = LocalizationManager.Instance.GetText(key);
    }
}