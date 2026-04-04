using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class AuraGlowController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Image characterImage; // основной персонаж
    [SerializeField] private Image auraImage;      // свечение/контур (отдельный Image)

    [Header("Palette (emotion -> color)")]
    [SerializeField] private List<EmotionAura> aura = new();

    [Header("Fallback")]
    [SerializeField] private string fallbackEmotion = "Calm";

    private Dictionary<string, EmotionAura> dict;
    private static readonly StringComparer Cmp = StringComparer.OrdinalIgnoreCase;

    private EmotionAura current;
    private float t;

    private void Awake()
    {
        dict = new Dictionary<string, EmotionAura>(Cmp);
        foreach (var a in aura)
        {
            if (string.IsNullOrWhiteSpace(a.emotion)) continue;
            dict[a.emotion.Trim()] = a;
        }

        // стартуем с Calm
        SetAura(fallbackEmotion);
    }

    private void LateUpdate()
    {
        if (auraImage == null) return;
        if (current == null) return;

        // пульсация
        t += Time.deltaTime * Mathf.Max(0.01f, current.pulseSpeed);
        float s = (Mathf.Sin(t * Mathf.PI * 2f) + 1f) * 0.5f; // 0..1

        float alpha = Mathf.Lerp(current.minAlpha, current.maxAlpha, s);
        float scale = Mathf.Lerp(current.minScale, current.maxScale, s);

        var c = current.color;
        c.a = alpha;
        auraImage.color = c;

        auraImage.rectTransform.localScale = new Vector3(scale, scale, 1f);

        // если используешь шейдер с параметром толщины/интенсивности — можно пульсить и его
        if (auraImage.material != null)
        {
            if (auraImage.material.HasProperty("_Glow"))
                auraImage.material.SetFloat("_Glow", Mathf.Lerp(current.minGlow, current.maxGlow, s));

            if (auraImage.material.HasProperty("_OutlineSize"))
                auraImage.material.SetFloat("_OutlineSize", Mathf.Lerp(current.minOutline, current.maxOutline, s));
        }
    }

    public void SetAura(string emotion)
    {
        if (auraImage == null || characterImage == null) return;

        // всегда держим спрайт ауры = спрайту персонажа
        auraImage.sprite = characterImage.sprite;

        EmotionAura a = null;

        if (!string.IsNullOrWhiteSpace(emotion) && dict.TryGetValue(emotion.Trim(), out var found))
            a = found;
        else if (dict.TryGetValue(fallbackEmotion, out var fb))
            a = fb;

        current = a;

        // если эмоции нет — просто выключим ауру
        auraImage.enabled = (current != null);
        t = 0f;
    }
}

[Serializable]
public class EmotionAura
{
    public string emotion;  // "Calm", "Happy", "Fear", ...

    public Color color = Color.white;

    [Header("Pulse")]
    public float pulseSpeed = 1.2f;
    public float minAlpha = 0.15f;
    public float maxAlpha = 0.35f;
    public float minScale = 1.00f;
    public float maxScale = 1.03f;

    [Header("If shader supports it")]
    public float minGlow = 0.6f;
    public float maxGlow = 1.1f;
    public float minOutline = 1.0f;
    public float maxOutline = 2.0f;
}
