using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EpisodeEndPanel : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI titleText;

    public TextMeshProUGUI trustAGText;
    public TextMeshProUGUI trustJAText;
    public TextMeshProUGUI riskText;
    public TextMeshProUGUI safetyText;
    public TextMeshProUGUI sparksText;

    [Header("Rows")]
    public GameObject trustAGRow;
    public GameObject trustJARow;
    public GameObject riskRow;
    public GameObject safetyRow;
    public GameObject sparksRow;

    [Header("Buttons")]
    public Button continueButton;
    [SerializeField] private TMP_Text continueButtonText;

    [Header("Colors")]
    public Color totalValueColor = new Color32(58, 52, 68, 255);   // #FDFDF9
    public Color positiveDeltaColor = new Color32(207, 167, 91, 255); // #CFA75B
    public Color negativeDeltaColor = new Color32(181, 84, 72, 255);  // #B55448

    [Header("Text Size")]
    [Range(100, 200)] public int totalSizePercent = 120;

    [Header("Title")]
    public float fixedTitleFontSize = 65f;

    private DialogueController dialogueController;

    public void Init(DialogueController controller)
    {
        dialogueController = controller;
    }

    public void Show(SaveData save, string nextEpisodePath, string nextEpisodeStartNode)
    {
        if (save == null)
        {
            Debug.LogError("[EpisodeEndPanel] SaveData is null.");
            return;
        }

        gameObject.SetActive(true);

        SetupTitle();

        var ep = TempGameContext.CurrentEpisode;

        SetupStatRow(trustAGRow, trustAGText, "trust_ag", save.trustAGTotal, ep.trustAG);
        SetupStatRow(trustJARow, trustJAText, "trust_ja", save.trustJATotal, ep.trustJA);
        SetupStatRow(riskRow, riskText, "risk", save.riskTotal, ep.risk);
        SetupStatRow(safetyRow, safetyText, "safety", save.safetyTotal, ep.safety);
        SetupStatRow(sparksRow, sparksText, "sparks", save.sparksTotal, ep.sparks);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                dialogueController?.StartNextEpisode(nextEpisodePath);
            });
        }

        if (continueButtonText != null)
        {
            continueButtonText.text =
                LocalizationManager.Instance.GetText("Episode", "continue");
        }

        Canvas.ForceUpdateCanvases();

        RectTransform rootRect = GetComponent<RectTransform>();
        if (rootRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetupTitle()
    {
        if (titleText == null)
            return;

        titleText.enableAutoSizing = false;
        titleText.fontSize = fixedTitleFontSize;
        titleText.text = LocalizationManager.Instance.GetText("Episode", "episode_end_title");

        titleText.enableWordWrapping = true;
        titleText.overflowMode = TextOverflowModes.Masking;
        titleText.alignment = TextAlignmentOptions.Center;

        titleText.ForceMeshUpdate();

        Debug.Log("[EpisodeEndPanel] Title font size set to: " + titleText.fontSize);
    }

    private void SetupStatRow(
        GameObject rowObject,
        TextMeshProUGUI textField,
        string statKey, // TODO: later replace with localization key lookup
        int totalValue,
        int deltaValue)
    {
        bool shouldShow = totalValue > 0;

        if (rowObject != null)
            rowObject.SetActive(shouldShow);

        if (!shouldShow || textField == null)
            return;

        textField.richText = true;
        textField.enableAutoSizing = false;
        textField.enableWordWrapping = true;
        textField.overflowMode = TextOverflowModes.Masking;

        string label = LocalizationManager.Instance.GetText("Stats", statKey);
        textField.text = FormatStatText(label, totalValue, deltaValue);
    }

    private string FormatStatText(string label, int totalValue, int deltaValue)
    {
        string totalHex = ColorUtility.ToHtmlStringRGB(totalValueColor);
        string positiveHex = ColorUtility.ToHtmlStringRGB(positiveDeltaColor);
        string negativeHex = ColorUtility.ToHtmlStringRGB(negativeDeltaColor);

        string totalPart = $"<size={totalSizePercent}%><color=#{totalHex}>{totalValue}</color></size>";

        string deltaPart = "";
        if (deltaValue > 0)
            deltaPart = $" <color=#{positiveHex}>(+{deltaValue})</color>";
        else if (deltaValue < 0)
            deltaPart = $" <color=#{negativeHex}>({deltaValue})</color>";

        string labelPart = $" <color=#{totalHex}>{label}</color>";

        return $"{totalPart}{deltaPart}{labelPart}";
    }
}