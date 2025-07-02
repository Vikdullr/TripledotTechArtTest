using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform panelRectTransform;

    void Awake()
    {
        panelRectTransform = GetComponent<RectTransform>();
    }
    
    void OnEnable()
    {
        ApplySafeArea();
    }

    void OnRectTransformDimensionsChange()
    {
        ApplySafeArea();
    }
    
    void ApplySafeArea()
    {
        if (panelRectTransform == null) return;
        
        Rect safeArea = Screen.safeArea;
        
        Vector2 anchorMin = new Vector2(safeArea.x / Screen.width,
                                        safeArea.y / Screen.height);
        Vector2 anchorMax = new Vector2((safeArea.x + safeArea.width) / Screen.width,
                                        (safeArea.y + safeArea.height) / Screen.height);
        
        panelRectTransform.anchorMin = anchorMin;
        panelRectTransform.anchorMax = anchorMax;
    }
}