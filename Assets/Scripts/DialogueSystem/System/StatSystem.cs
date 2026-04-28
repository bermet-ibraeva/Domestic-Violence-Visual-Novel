using System.Collections.Generic;
using UnityEngine;

public class StatSystem : MonoBehaviour
{
    [SerializeField] private StatFeedbackUI statFeedbackUI;

    public static StatSystem Instance;

    private SaveData saveData;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Init(SaveData data)
    {
        saveData = data;
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
        if (effect == null) return;

        int value = effect.op switch
        {
            "inc" => effect.value,
            "dec" => -effect.value,
            "set" => 0,
            _ => 0
        };

        if (effect.op == "set")
        {
            SetStat(effect.key, effect.value);
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
        if (saveData == null)
        {
            Debug.LogError("[StatSystem] SaveData is null.");
            return;
        }

        var ep = TempGameContext.CurrentEpisode;
        if (ep == null)
        {
            Debug.LogError("[StatSystem] CurrentEpisode is null.");
            return;
        }

        switch (key)
        {
            case "trust.AG":
            case "trustAG":
                saveData.trustAGTotal += value;
                ep.trustAG += value;
                LogStat("stat_trustAG", value, ep.trustAG, saveData.trustAGTotal);
                break;

            case "trust.JA":
            case "trustJA":
                saveData.trustJATotal += value;
                ep.trustJA += value;
                LogStat("stat_trustJA", value, ep.trustJA, saveData.trustJATotal);
                break;

            case "risk":
                saveData.riskTotal += value;
                ep.risk += value;
                LogStat("stat_risk", value, ep.risk, saveData.riskTotal);
                break;

            case "safety":
                saveData.safetyTotal += value;
                ep.safety += value;
                LogStat("stat_safety", value, ep.safety, saveData.safetyTotal);
                break;

            case "sparks":
                saveData.sparksTotal += value;
                ep.sparks += value;
                LogStat("stat_sparks", value, ep.sparks, saveData.sparksTotal);
                break;

            default:
                Debug.LogWarning($"[StatSystem] Unknown stat key: {key}");
                break;
        }

        // ❗ НЕ показываем sparks
        if (statFeedbackUI != null && key != "sparks")
        {
            statFeedbackUI.ShowStatChange(key, value);
        }
    }

    private void SetStat(string key, int value)
    {
        if (saveData == null) return;

        var ep = TempGameContext.CurrentEpisode;
        if (ep == null) return;

        key = key?.Trim();

        switch (key)
        {
            case "trustAG":
                ep.trustAG += value - saveData.trustAGTotal;
                saveData.trustAGTotal = value;
                break;

            case "trustJA":
                ep.trustJA += value - saveData.trustJATotal;
                saveData.trustJATotal = value;
                break;

            case "risk":
                ep.risk += value - saveData.riskTotal;
                saveData.riskTotal = value;
                break;

            case "safety":
                ep.safety += value - saveData.safetyTotal;
                saveData.safetyTotal = value;
                break;

            case "sparks":
                ep.sparks += value - saveData.sparksTotal;
                saveData.sparksTotal = value;
                break;
        }
    }

    // ================= DEBUG =================

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
}