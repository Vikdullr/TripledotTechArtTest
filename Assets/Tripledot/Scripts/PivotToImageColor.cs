using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PivotToImageColor : MonoBehaviour
{
    void Start()
    {
        Image image = GetComponent<Image>();
        RectTransform rectTransform = image.rectTransform;
        
        Vector2 pivot = rectTransform.pivot;
        
        image.color = new Color(pivot.x, pivot.y, 1f, 1f);
    }
}