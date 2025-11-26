using UnityEngine;

public static class DialogueTimer
{
    // Рассчитывает длительность показа текста по количеству слов и эмоции
    public static float GetDuration(string text, string emotion)
    {
        int wordCount = text.Split(' ').Length;
        float baseTime = wordCount * 0.5f; // 0.5 секунды на слово

        switch (emotion)
        {
            case "Scared": baseTime *= 1.2f; break;
            case "Sad": baseTime *= 1.1f; break;
            case "Happy": baseTime *= 1f; break;
            case "Calm": baseTime *= 1f; break;
        }

        return Mathf.Clamp(baseTime, 2f, 12f); // минимум 2 сек, максимум 12 сек
    }
}
