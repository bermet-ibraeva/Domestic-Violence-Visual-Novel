using TMPro;
using UnityEngine;

public class SparksBadgeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text sparksText;

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        SaveData.OnSparksChanged += Refresh;
    }

    private void OnDisable()
    {
        SaveData.OnSparksChanged -= Refresh;
    }

    private void Refresh()
    {
        if (SaveManager.Instance == null)
            return;

        if (SaveManager.Instance.Data == null)
            return;

        if (sparksText != null)
        {
            sparksText.text =
                SaveManager.Instance.Data.sparksTotal.ToString();
        }
    }
}