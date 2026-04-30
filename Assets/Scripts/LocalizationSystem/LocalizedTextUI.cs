using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedTextUI : MonoBehaviour
{
    [SerializeField] private string key;

    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        // ждём пока LocalizationManager готов
        while (LocalizationManager.Instance == null || !LocalizationManager.Instance.IsLoaded)
            yield return null;

        UpdateText();

        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
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
        if (LocalizationManager.Instance == null)
            return;

        textComponent.text = LocalizationManager.Instance.GetText(key);
    }
}