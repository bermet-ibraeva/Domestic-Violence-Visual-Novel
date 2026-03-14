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
        if (saveData == null) return;

        saveData.episodeRisk = 0;
        saveData.episodeSafety = 0;
        saveData.episodeTrustAG = 0;
        saveData.episodeTrustJA = 0;
        saveData.episodeSparks = 0;

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

        switch (key)
        {
            case "trust.AG":
            case "trustAG":
                saveData.trustAG += value;
                saveData.episodeTrustAG += value;
                LogStat("Доверие AG", value, saveData.episodeTrustAG, saveData.trustAG);
                break;

            case "trust.JA":
            case "trustJA":
                saveData.trustJA += value;
                saveData.episodeTrustJA += value;
                LogStat("Доверие JA", value, saveData.episodeTrustJA, saveData.trustJA);
                break;

            case "risk":
                saveData.riskTotal += value;
                saveData.episodeRisk += value;
                LogStat("Риск", value, saveData.episodeRisk, saveData.riskTotal);
                break;

            case "safety":
                saveData.safetyTotal += value;
                saveData.episodeSafety += value;
                LogStat("Безопасность", value, saveData.episodeSafety, saveData.safetyTotal);
                break;

            case "sparks":
                saveData.sparksTotal += value;
                saveData.episodeSparks += value;
                LogStat("Искры", value, saveData.episodeSparks, saveData.sparksTotal);
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

        key = key?.Trim();

        switch (key)
        {
            case "trustAG":
                saveData.episodeTrustAG += value - saveData.trustAG;
                saveData.trustAG = value;
                break;

            case "trustJA":
                saveData.episodeTrustJA += value - saveData.trustJA;
                saveData.trustJA = value;
                break;

            case "risk":
                saveData.episodeRisk += value - saveData.riskTotal;
                saveData.riskTotal = value;
                break;

            case "safety":
                saveData.episodeSafety += value - saveData.safetyTotal;
                saveData.safetyTotal = value;
                break;

            case "sparks":
                saveData.episodeSparks += value - saveData.sparksTotal;
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

    public string BuildEpisodeSummary()
    {
        if (saveData == null) return "SaveData is null";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== ИТОГ ЭПИЗОДА =====");
        sb.AppendLine($"Искры: {FormatSigned(saveData.episodeSparks)}");
        sb.AppendLine($"Доверие Айназ <-> Гульдана: {FormatSigned(saveData.episodeTrustAG)}");
        sb.AppendLine($"Доверие Жамиля <-> Аида: {FormatSigned(saveData.episodeTrustJA)}");
        sb.AppendLine($"Риск: {FormatSigned(saveData.episodeRisk)}");
        sb.AppendLine($"Безопасность: {FormatSigned(saveData.episodeSafety)}");
        sb.AppendLine();
        sb.AppendLine("===== ОБЩИЕ ЗНАЧЕНИЯ =====");
        sb.AppendLine($"Всего искр: {saveData.sparksTotal}");
        sb.AppendLine($"Всего доверия Айназ <-> Гульдана: {saveData.trustAG}");
        sb.AppendLine($"Всего доверия Жамиля <-> Аида: {saveData.trustJA}");
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