using System.Collections.Generic;
using UnityEngine;
using System.Text;


// score system applier
public class StatSystem : MonoBehaviour
{
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

    public void ApplyChoiceEffects(List<EffectOp> effects)
    {
        if (effects == null || effects.Count == 0) return;

        foreach (var effect in effects)
        {
            ApplyEffect(effect);
        }
    }

    public void ApplyLegacyNodeEffects(NodeEffects effects)
    {
        if (effects == null) return;

        if (effects.trustAG != 0)
            AddStat("trustAG", effects.trustAG);

        if (effects.trustJA != 0)
            AddStat("trustJA", effects.trustJA);

        if (effects.risk != 0)
            AddStat("risk", effects.risk);

        if (effects.safety != 0)
            AddStat("safety", effects.safety);

        if (effects.sparks != 0)
            AddStat("sparks", effects.sparks);
    }

    public void ApplyEffect(EffectOp effect)
    {
        if (effect == null) return;

        int finalValue = effect.value;

        switch (effect.op)
        {
            case "inc":
                finalValue = effect.value;
                break;

            case "dec":
                finalValue = -effect.value;
                break;

            case "set":
                SetStat(effect.key, effect.value);
                return;

            default:
                Debug.LogWarning($"[StatSystem] Unknown op: {effect.op}");
                return;
        }

        AddStat(effect.key, finalValue);
    }

    private void AddStat(string key, int value)
    {
        if (saveData == null)
        {
            Debug.LogError("[StatSystem] SaveData is null.");
            return;
        }

        var ep = TempGameContext.CurrentEpisode;

        switch (key)
        {
            case "trust.AG":
            case "trustAG":
                saveData.trustAGTotal += value;
                ep.trustAG += value;
                LogStat("Доверие AG", value, ep.trustAG, saveData.trustAGTotal);
                break;

            case "trust.JA":
            case "trustJA":
                saveData.trustJATotal += value;
                ep.trustJA += value;
                LogStat("Доверие JA", value, ep.trustJA, saveData.trustJATotal);
                break;

            case "risk":
                saveData.riskTotal += value;
                ep.risk += value;
                LogStat("Риск", value, ep.risk, saveData.riskTotal);
                break;

            case "safety":
                saveData.safetyTotal += value;
                ep.safety += value;
                LogStat("Безопасность", value, ep.safety, saveData.safetyTotal);
                break;

            case "sparks":
                saveData.sparksTotal += value;
                ep.sparks += value;
                LogStat("Искры", value, ep.sparks, saveData.sparksTotal);
                break;

            default:
                Debug.LogWarning($"[StatSystem] Unknown stat key: {key}");
                break;
        }
    }
    
    private void SetStat(string key, int value)
    {
        if (saveData == null)
        {
            Debug.LogError("[StatSystem] SaveData is null.");
            return;
        }

        var ep = TempGameContext.CurrentEpisode;
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

            default:
                Debug.LogWarning($"[StatSystem] Unknown stat key for set: {key}");
                break;
        }
    }

    public void AddEpisodeReward(int sparksReward)
    {
        AddStat("sparks", sparksReward);
        Debug.Log($"[StatSystem] Награда за эпизод: +{sparksReward} искр");
    }

    // not really being used, but can be helpful for debugging 
    public string BuildEpisodeSummary()
    {
        if (saveData == null) return "SaveData is null";

        var ep = TempGameContext.CurrentEpisode;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== ИТОГ ЭПИЗОДА =====");
        sb.AppendLine($"Искры: {FormatSigned(ep.sparks)}");
        sb.AppendLine($"Доверие Айназ <-> Гульдана: {FormatSigned(ep.trustAG)}");
        sb.AppendLine($"Доверие Жамиля <-> Аида: {FormatSigned(ep.trustJA)}");
        sb.AppendLine($"Риск: {FormatSigned(ep.risk)}");
        sb.AppendLine($"Безопасность: {FormatSigned(ep.safety)}");
        sb.AppendLine();
        sb.AppendLine("===== ОБЩИЕ ЗНАЧЕНИЯ =====");
        sb.AppendLine($"Всего искр: {saveData.sparksTotal}");
        sb.AppendLine($"Всего доверия Айназ <-> Гульдана: {saveData.trustAGTotal}");
        sb.AppendLine($"Всего доверия Жамиля <-> Аида: {saveData.trustJATotal}");
        sb.AppendLine($"Всего риска: {saveData.riskTotal}");
        sb.AppendLine($"Всего безопасности: {saveData.safetyTotal}");

        return sb.ToString();
    }

    public void PrintEpisodeSummary()
    {
        Debug.Log(BuildEpisodeSummary());
    }

    private void LogStat(string label, int delta, int episodeValue, int totalValue)
    {
        Debug.Log($"[StatSystem] {label}: {FormatSigned(delta)} | За эпизод: {episodeValue} | Всего: {totalValue}");
    }

    private string FormatSigned(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }
}