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

    public void Show(
        SaveData save,
        string nextEpisodePath,
        string nextEpisodeStartNode)
    {
        if (save == null)
        {
            Debug.LogError("[EpisodeEndPanel] SaveData is null.");
            return;
        }

        gameObject.SetActive(true);

        SetupTitle();

        SetupStatRow(trustAGRow, trustAGText, "trust_ag", save.trustAG, save.episodeTrustAG);
        SetupStatRow(trustJARow, trustJAText, "trust_ja", save.trustJA, save.episodeTrustJA);
        SetupStatRow(riskRow, riskText, "risk", save.riskTotal, save.episodeRisk);
        SetupStatRow(safetyRow, safetyText, "safety", save.safetyTotal, save.episodeSafety);
        SetupStatRow(sparksRow, sparksText, "sparks", save.sparksTotal, save.episodeSparks);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                dialogueController?.StartNextEpisode(nextEpisodePath, nextEpisodeStartNode);
            });
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
        titleText.text = "ЭПИЗОД ЗАВЕРШЁН!"; // TODO: replace with Localization.Get("episode_end_title")

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

        string label = GetTempLabel(statKey); // TODO: replace with Localization.Get(statKey)
        textField.text = FormatStatText(label, totalValue, deltaValue);
    }

    private string GetTempLabel(string statKey)
    {
        // TODO: replace with proper localization system later
        switch (statKey)
        {
            case "trust_ag":
                return "Доверие Айназ и Гульданы";

            case "trust_ja":
                return "Доверие Жамили и Айды";

            case "risk":
                return "Риск";

            case "safety":
                return "Безопасность";

            case "sparks":
                return "Искорки";

            default:
                return "Стат";
        }
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