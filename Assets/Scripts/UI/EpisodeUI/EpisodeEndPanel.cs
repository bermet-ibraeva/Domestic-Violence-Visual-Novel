using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EpisodeEndPanel : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI titleText;

    [SerializeField] private TextMeshProUGUI trustAGText;
    [SerializeField] private TextMeshProUGUI trustJAText;
    [SerializeField] private TextMeshProUGUI riskText;
    [SerializeField] private TextMeshProUGUI safetyText;
    [SerializeField] private TextMeshProUGUI sparksText;

    [Header("Rows")]
    [SerializeField] private GameObject trustAGRow;
    [SerializeField] private GameObject trustJARow;
    [SerializeField] private GameObject riskRow;
    [SerializeField] private GameObject safetyRow;
    [SerializeField] private GameObject sparksRow;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueButtonText;

    [Header("Colors")]
    [SerializeField] private Color totalValueColor = new Color32(58, 52, 68, 255);
    [SerializeField] private Color positiveDeltaColor = new Color32(207, 167, 91, 255);
    [SerializeField] private Color negativeDeltaColor = new Color32(181, 84, 72, 255);

    [Header("Text Size")]
    [Range(100, 200)]
    [SerializeField] private int totalSizePercent = 120;

    [Header("Title")]
    [SerializeField] private float fixedTitleFontSize = 65f;

    private DialogueController dialogueController;

    public void Init(DialogueController controller)
    {
        dialogueController = controller;
    }

    public void Show(string nextEpisodePath)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[EpisodeEndPanel] SaveManager is NULL");
            return;
        }

        if (SaveManager.Instance.Data == null)
        {
            Debug.LogError("[EpisodeEndPanel] SaveManager is NULL");
            return;
        }

        var ep = TempGameContext.CurrentEpisode;

        if (ep == null)
        {
            Debug.LogError("[EpisodeEndPanel] CurrentEpisode is NULL");
            return;
        }

        gameObject.SetActive(true);

        SetupTitle();

        SetupStatRow(
            trustAGRow,
            trustAGText,
            "trust_ag",
            SaveManager.Instance.Data.trustAGTotal,
            ep.trustAG
        );

        SetupStatRow(
            trustJARow,
            trustJAText,
            "trust_ja",
            SaveManager.Instance.Data.trustJATotal,
            ep.trustJA
        );

        SetupStatRow(
            riskRow,
            riskText,
            "risk",
            SaveManager.Instance.Data.riskTotal,
            ep.risk
        );

        SetupStatRow(
            safetyRow,
            safetyText,
            "safety",
            SaveManager.Instance.Data.safetyTotal,
            ep.safety
        );

        SetupStatRow(
            sparksRow,
            sparksText,
            "sparks",
            SaveManager.Instance.Data.sparksTotal,
            ep.sparks
        );

        SetupContinueButton(nextEpisodePath);

        Canvas.ForceUpdateCanvases();

        RectTransform rootRect = GetComponent<RectTransform>();

        if (rootRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // ================= TITLE =================
    private void SetupTitle()
    {
        if (titleText == null)
            return;

        titleText.enableAutoSizing = false;
        titleText.fontSize = fixedTitleFontSize;

        titleText.text =
            LocalizationManager.Instance.GetText("Episode", "episode_end_title");

        titleText.enableWordWrapping = true;
        titleText.overflowMode = TextOverflowModes.Masking;
        titleText.alignment = TextAlignmentOptions.Center;

        titleText.ForceMeshUpdate();
    }

    // ================= BUTTON =================
    private void SetupContinueButton(string nextEpisodePath)
    {
        if (continueButton == null)
            return;

        continueButton.onClick.RemoveAllListeners();

        bool hasNextEpisode =
            !string.IsNullOrEmpty(nextEpisodePath);

        continueButton.gameObject.SetActive(hasNextEpisode);

        if (!hasNextEpisode)
            return;

        continueButton.onClick.AddListener(() =>
        {
            dialogueController?.StartNextEpisode(nextEpisodePath);
        });

        if (continueButtonText != null)
        {
            continueButtonText.text =
                LocalizationManager.Instance.GetText(
                    "Episode",
                    "continue"
                );
        }
    }

    // ================= STATS =================
    private void SetupStatRow(
        GameObject rowObject,
        TextMeshProUGUI textField,
        string statKey,
        int totalValue,
        int deltaValue)
    {
        bool shouldShow = totalValue > 0;

        if (rowObject != null)
        {
            rowObject.SetActive(shouldShow);
        }

        if (!shouldShow || textField == null)
            return;

        textField.richText = true;
        textField.enableAutoSizing = false;
        textField.enableWordWrapping = true;
        textField.overflowMode = TextOverflowModes.Masking;

        string label =
            LocalizationManager.Instance.GetText(
                "Stats",
                statKey
            );

        textField.text =
            FormatStatText(label, totalValue, deltaValue);
    }

    private string FormatStatText(
        string label,
        int totalValue,
        int deltaValue)
    {
        string totalHex =
            ColorUtility.ToHtmlStringRGB(totalValueColor);

        string positiveHex =
            ColorUtility.ToHtmlStringRGB(positiveDeltaColor);

        string negativeHex =
            ColorUtility.ToHtmlStringRGB(negativeDeltaColor);

        string totalPart =
            $"<size={totalSizePercent}%><color=#{totalHex}>{totalValue}</color></size>";

        string deltaPart = "";

        if (deltaValue > 0)
        {
            deltaPart =
                $" <color=#{positiveHex}>(+{deltaValue})</color>";
        }
        else if (deltaValue < 0)
        {
            deltaPart =
                $" <color=#{negativeHex}>({deltaValue})</color>";
        }

        string labelPart =
            $" <color=#{totalHex}>{label}</color>";

        return $"{totalPart}{deltaPart}{labelPart}";
    }
}