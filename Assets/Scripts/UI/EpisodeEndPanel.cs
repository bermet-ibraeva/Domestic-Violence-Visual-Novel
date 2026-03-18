using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EpisodeEndPanel : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;

    [Header("Texts")]
    public TextMeshProUGUI titleText;

    public TextMeshProUGUI trustAGText;
    public TextMeshProUGUI trustJAText;
    public TextMeshProUGUI riskText;
    public TextMeshProUGUI safetyText;
    public TextMeshProUGUI sparksText;

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
        int sparksReward,
        string nextEpisodePath,
        string nextEpisodeStartNode)
    {
        if (save == null)
        {
            Debug.LogError("[EpisodeEndPanel] SaveData is null.");
            return;
        }

        if (root != null)
            root.SetActive(true);

        if (titleText != null)
            titleText.text = "Эпизод закончен!";

        if (trustAGText != null)
            trustAGText.text = BuildStatLine(
                save.trustAG,
                save.episodeTrustAG,
                "Доверие между Айназ и Гульданой"
            );

        if (trustJAText != null)
            trustJAText.text = BuildStatLine(
                save.trustJA,
                save.episodeTrustJA,
                "Доверие между Аидой и Жамилёй"
            );

        if (riskText != null)
            riskText.text = BuildStatLine(
                save.riskTotal,
                save.episodeRisk,
                "Риск"
            );

        if (safetyText != null)
            safetyText.text = BuildStatLine(
                save.safetyTotal,
                save.episodeSafety,
                "Безопасность"
            );

        if (sparksText != null)
            sparksText.text = BuildStatLine(
                save.sparksTotal,
                sparksReward,
                "Искры"
            );

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

    private string BuildStatLine(int totalValue, int deltaValue, string label)
    {
        if (deltaValue > 0)
            return $"{totalValue} (+{deltaValue})  {label}";

        if (deltaValue < 0)
            return $"{totalValue} ({deltaValue})  {label}";

        return $"{totalValue}  {label}";
    }
}