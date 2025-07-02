using UnityEngine;
using UnityEngine.Rendering;

public class BlurAnimator : MonoBehaviour
{
    public Volume postProcessVolume;

    private BlurVolumeComponent _blurComponent;

    void Awake()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = GetComponent<Volume>();
        }

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out _blurComponent);
        }
    }

    public void SetBlurMultiplier(float value)
    {
        if (_blurComponent != null)
        {
            _blurComponent.blurMultiplier.value = value;
        }
    }

    public void SetHorizontalBlur(float value)
    {
        if (_blurComponent != null)
        {
            _blurComponent.horizontalBlur.value = value;
        }
    }

    public void SetVerticalBlur(float value)
    {
        if (_blurComponent != null)
        {
            _blurComponent.verticalBlur.value = value;
        }
    }
}