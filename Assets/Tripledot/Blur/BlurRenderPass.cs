using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRenderPass : ScriptableRenderPass
{
    private static readonly int horizontalBlurId =
        Shader.PropertyToID("_HorizontalBlur");
    private static readonly int verticalBlurId =
        Shader.PropertyToID("_VerticalBlur");

    private BlurSettings defaultSettings;
    private Material material;

    private RenderTextureDescriptor blurTextureDescriptor;
    private RTHandle blurTextureHandle;

    public BlurRenderPass(Material material, BlurSettings defaultSettings)
    {
        this.material = material;
        this.defaultSettings = defaultSettings;

        blurTextureDescriptor = new RenderTextureDescriptor(Screen.width,
            Screen.height, RenderTextureFormat.Default, 0);
    }

    public override void Configure(CommandBuffer cmd,
        RenderTextureDescriptor cameraTextureDescriptor)
    {
        blurTextureDescriptor.width = cameraTextureDescriptor.width;
        blurTextureDescriptor.height = cameraTextureDescriptor.height;

        RenderingUtils.ReAllocateIfNeeded(ref blurTextureHandle, blurTextureDescriptor);
    }

    private void UpdateBlurSettings()
    {
        if (material == null)
        {
            return;
        }

        var volumeComponent =
            VolumeManager.instance.stack.GetComponent<BlurVolumeComponent>();

        bool useVolume = volumeComponent.horizontalBlur.overrideState ||
                         volumeComponent.verticalBlur.overrideState ||
                         volumeComponent.blurMultiplier.overrideState;

        if (useVolume)
        {
            material.SetFloat(horizontalBlurId, volumeComponent.GetHorizontalBlur());
            material.SetFloat(verticalBlurId, volumeComponent.GetVerticalBlur());
        }
        else
        {
            material.SetFloat(horizontalBlurId, defaultSettings.horizontalBlur);
            material.SetFloat(verticalBlurId, defaultSettings.verticalBlur);
        }
    }

    public override void Execute(ScriptableRenderContext context,
        ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        RTHandle cameraTargetHandle =
            renderingData.cameraData.renderer.cameraColorTargetHandle;

        UpdateBlurSettings();

        Blit(cmd, cameraTargetHandle, blurTextureHandle, material, 0);
        Blit(cmd, blurTextureHandle, cameraTargetHandle, material, 1);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        blurTextureHandle?.Release();
    }
}