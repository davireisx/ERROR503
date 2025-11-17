using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaUI : MonoBehaviour
{
    void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Rect safe = Screen.safeArea;

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
    }
}
