using UnityEngine;

public static class TextFormatter
{
    public static string Format(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        text = text.Replace(
            "<bold>",
            "<font=\"Roboto-Bold SDF\">"
        );

        text = text.Replace(
            "</bold>",
            "</font>"
        );

        return text;
    }
}