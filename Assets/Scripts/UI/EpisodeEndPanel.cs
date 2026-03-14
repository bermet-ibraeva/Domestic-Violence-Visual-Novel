using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EpisodeEndPanel : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;

    [Header("Texts")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI endingText;
    public TextMeshProUGUI rewardText;

    public TextMeshProUGUI trustText;
    public TextMeshProUGUI riskText;
    public TextMeshProUGUI safetyText;
    public TextMeshProUGUI sparksText;

    public TextMeshProUGUI totalTrustText;
    public TextMeshProUGUI totalRiskText;
    public TextMeshProUGUI totalSafetyText;
    public TextMeshProUGUI totalSparksText;

    [Header("Buttons")]
    public Button nextEpisodeButton;
    public Button restartEpisodeButton;

    private DialogueController dialogueController;

    public void Init(DialogueController controller)
    {
        dialogueController = controller;
    }

    public void Show(
        SaveData save,
        string endingName,
        int completionReward,
        string nextEpisodePath,
        string nextEpisodeStartNode)
    {
        if (root != null)
            root.SetActive(true);

        if (titleText != null)
            titleText.text = "Эпизод завершён";

        if (endingText != null)
            endingText.text = $"Финал: {endingName}";

        if (rewardText != null)
            rewardText.text = $"+{completionReward} искры за завершение эпизода";

        if (trustText != null)
            trustText.text = $"Доверие: {FormatSigned(save.episodeTrustAG)}";

        if (riskText != null)
            riskText.text = $"Риск: {FormatSigned(save.episodeRisk)}";

        if (safetyText != null)
            safetyText.text = $"Безопасность: {FormatSigned(save.episodeSafety)}";

        if (sparksText != null)
            sparksText.text = $"Искры: {FormatSigned(save.episodeSparks)}";

        if (totalTrustText != null)
            totalTrustText.text = $"Всего доверия: {save.trustAG}";

        if (totalRiskText != null)
            totalRiskText.text = $"Всего риска: {save.riskTotal}";

        if (totalSafetyText != null)
            totalSafetyText.text = $"Всего безопасности: {save.safetyTotal}";

        if (totalSparksText != null)
            totalSparksText.text = $"Всего искр: {save.sparksTotal}";

        if (nextEpisodeButton != null)
        {
            nextEpisodeButton.onClick.RemoveAllListeners();
            nextEpisodeButton.onClick.AddListener(() =>
            {
                dialogueController?.StartNextEpisode(nextEpisodePath, nextEpisodeStartNode);
            });
        }

        if (restartEpisodeButton != null)
        {
            restartEpisodeButton.onClick.RemoveAllListeners();
            restartEpisodeButton.onClick.AddListener(() =>
            {
                dialogueController?.RestartCurrentEpisode();
            });
        }
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    private string FormatSigned(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }
}