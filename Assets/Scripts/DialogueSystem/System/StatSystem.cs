using System.Collections.Generic;
using UnityEngine;

public class StatSystem : MonoBehaviour
{
    [SerializeField] private StatFeedbackUI statFeedbackUI;

    public static StatSystem Instance;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ResetEpisodeStats()
    {
        TempGameContext.ResetEpisode();
        Debug.Log("[StatSystem] Episode stats reset.");
    }


    // ================= APPLY =================
    public void ApplyChoiceEffects(List<EffectOp> effects)
    {
        if (effects == null || effects.Count == 0) return;

        foreach (var effect in effects)
            ApplyEffect(effect);
    }

    public void ApplyLegacyNodeEffects(NodeEffects effects)
    {
        if (effects == null) return;

        if (effects.trustAG != 0) AddStat("trustAG", effects.trustAG);
        if (effects.trustJA != 0) AddStat("trustJA", effects.trustJA);
        if (effects.risk != 0) AddStat("risk", effects.risk);
        if (effects.safety != 0) AddStat("safety", effects.safety);
        if (effects.sparks != 0) AddStat("sparks", effects.sparks);
    }

    public void ApplyEffect(EffectOp effect)
    {
        if (effect == null)
            return;

        int value = effect.op switch
        {
            "inc" => effect.value,
            "dec" => -effect.value,
            "set" => effect.value,
            _ => 0
        };

        if (effect.op == "set")
        {
            SetStat(effect.key, value);
            return;
        }

        if (effect.op != "inc" && effect.op != "dec")
        {
            Debug.LogWarning($"[StatSystem] Unknown op: {effect.op}");
            return;
        }

        AddStat(effect.key, value);
    }


    // ================= CORE =================
    private void AddStat(string key, int value)
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
        {
            Debug.LogError("[StatSystem] SaveManager.Instance.Data is null.");
            return;
        }

        var ep = TempGameContext.CurrentEpisode;
        if (ep == null)
        {
            Debug.LogError("[StatSystem] CurrentEpisode is null.");
            return;
        }

        key = NormalizeKey(key);

        switch (key)
        {
            case "trustAG":
                SaveManager.Instance.Data.trustAGTotal += value;
                ep.trustAG += value;
                LogStat("stat_trustAG", value, ep.trustAG, SaveManager.Instance.Data.trustAGTotal);
                break;

            case "trustJA":
                SaveManager.Instance.Data.trustJATotal += value;
                ep.trustJA += value;
                LogStat("stat_trustJA", value, ep.trustJA, SaveManager.Instance.Data.trustJATotal);
                break;

            case "risk":
                SaveManager.Instance.Data.riskTotal += value;
                ep.risk += value;
                LogStat("stat_risk", value, ep.risk, SaveManager.Instance.Data.riskTotal);
                break;

            case "safety":
                SaveManager.Instance.Data.safetyTotal += value;
                ep.safety += value;
                LogStat("stat_safety", value, ep.safety, SaveManager.Instance.Data.safetyTotal);
                break;

            case "sparks":
                SaveManager.Instance.Data.AddSparks(value);
                ep.sparks += value;
                LogStat("stat_sparks", value, ep.sparks, SaveManager.Instance.Data.sparksTotal);
                break;

            default:
                Debug.LogWarning($"[StatSystem] Unknown stat key: {key}");
                break;
        }

        if (statFeedbackUI != null && key != "sparks")
        {
            statFeedbackUI.ShowStatChange(key, value);
        }
    }

    private void SetStat(string key, int value)
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
            return;

        var ep = TempGameContext.CurrentEpisode;

        if (ep == null)
            return;

        key = NormalizeKey(key);

        switch (key)
        {
            case "trustAG":
                SaveManager.Instance.Data.trustAGTotal = value;
                ep.trustAG = value;
                LogStat("stat_trustAG", value, ep.trustAG, SaveManager.Instance.Data.trustAGTotal);
                break;

            case "trustJA":
                SaveManager.Instance.Data.trustJATotal = value;
                ep.trustJA = value;
                LogStat("stat_trustJA", value, ep.trustJA, SaveManager.Instance.Data.trustJATotal);
                break;

            case "risk":
                SaveManager.Instance.Data.riskTotal = value;
                ep.risk = value;
                LogStat("stat_risk", value, ep.risk, SaveManager.Instance.Data.riskTotal);
                break;

            case "safety":
                SaveManager.Instance.Data.safetyTotal = value;
                ep.safety = value;
                LogStat("stat_safety", value, ep.safety, SaveManager.Instance.Data.safetyTotal);
                break;

            case "sparks":
                SaveManager.Instance.Data.SetSparks(value);
                ep.sparks = value;
                LogStat("stat_sparks", value, ep.sparks, SaveManager.Instance.Data.sparksTotal);
                break;

            default:
                Debug.LogWarning($"[StatSystem] Unknown stat key: {key}");
                break;
        }
    }


    // ================= HELPERS =================
    private string NormalizeKey(string key)
    {
        return key?.Replace(".", "").Trim();
    }

    public void AddEpisodeReward(int sparksReward)
    {
        AddStat("sparks", sparksReward);

        string msg = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.GetText("Stats", "reward_episode")
            : "Episode reward";

        Debug.Log($"[StatSystem] {msg}: +{sparksReward}");
    }

    private string GetText(string key)
    {
        if (LocalizationManager.Instance == null)
            return key;

        return LocalizationManager.Instance.GetText("Stats", key);
    }

    private void LogStat(string key, int delta, int episodeValue, int totalValue)
    {
        string label = GetText(key);
        Debug.Log($"[StatSystem] {label}: {FormatSigned(delta)} | Эпизод: {episodeValue} | Всего: {totalValue}");
    }

    private string FormatSigned(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}