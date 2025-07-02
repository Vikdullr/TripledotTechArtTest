using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RectSizeToMaterial : MonoBehaviour
{
    private Image _image;
    private RectTransform _rectTransform;
    private Material _materialInstance;

    private static readonly int RectSizeProperty = Shader.PropertyToID("_RectSize");

    private Vector2 _lastSize;

    void Awake()
    {
        _image = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();

        _materialInstance = _image.material;
    }

    void Update()
    {
        Vector2 currentSize = _rectTransform.rect.size;

        if (currentSize != _lastSize)
        {
            if (_materialInstance != null)
            {
                _materialInstance.SetVector(RectSizeProperty, new Vector4(currentSize.x, currentSize.y, 0, 0));
            }
            _lastSize = currentSize;
        }
    }

    void OnDestroy()
    {
        if (_materialInstance != null)
        {
            Destroy(_materialInstance);
        }
    }
}