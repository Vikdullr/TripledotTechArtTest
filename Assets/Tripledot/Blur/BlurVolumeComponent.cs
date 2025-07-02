using System;
using UnityEngine.Rendering;

[Serializable]
public class BlurVolumeComponent : VolumeComponent
{
    public BoolParameter isActive = new BoolParameter(true);
    public ClampedFloatParameter blurMultiplier =
        new ClampedFloatParameter(1f, 0, 1);
    public ClampedFloatParameter horizontalBlur =
        new ClampedFloatParameter(0.05f, 0, 0.5f);
    public ClampedFloatParameter verticalBlur =
        new ClampedFloatParameter(0.05f, 0, 0.5f);

    public float GetHorizontalBlur()
    {
        return horizontalBlur.value * blurMultiplier.value;
    }

    public float GetVerticalBlur()
    {
        return verticalBlur.value * blurMultiplier.value;
    }
}