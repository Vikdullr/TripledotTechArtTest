using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaOffset : MonoBehaviour
{
    public enum OffsetSource { None, Top, Bottom }
    public enum OffsetDirection { Positive = 1, Negative = -1 }

    public OffsetSource source = OffsetSource.None;
    public OffsetDirection direction = OffsetDirection.Negative;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 initialAnchoredPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        initialAnchoredPosition = rectTransform.anchoredPosition;

        if (canvas == null)
        {
            Debug.LogError("SafeAreaOffset requires a Canvas in its parent hierarchy.", this);
            enabled = false;
        }
    }

    void OnEnable()
    {
        ApplyOffset();
    }
    
    void OnRectTransformDimensionsChange()
    {
        ApplyOffset();
    }

    void ApplyOffset()
    {
        if (rectTransform == null || canvas == null || source == OffsetSource.None) return;

        float scaleFactor = canvas.scaleFactor;
        float offsetValue = 0f;

        switch (source)
        {
            case OffsetSource.Top:
                offsetValue = (Screen.height - Screen.safeArea.yMax) / scaleFactor;
                break;
            case OffsetSource.Bottom:
                offsetValue = Screen.safeArea.yMin / scaleFactor;
                break;
        }

        Vector2 finalOffset = new Vector2(0, offsetValue * (int)direction);
        rectTransform.anchoredPosition = initialAnchoredPosition + finalOffset;
    }
}