using UnityEngine;

public class SafeArea : MonoBehaviour
{
    void Start()
    {
        Rect safeArea = Screen.safeArea;

        RectTransform rt = GetComponent<RectTransform>();

        Vector2 min = safeArea.position;
        Vector2 max = safeArea.position + safeArea.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        rt.anchorMin = min;
        rt.anchorMax = max;
    }
}