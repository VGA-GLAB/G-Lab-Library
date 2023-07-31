using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OutlineRenderFeature : ScriptableRendererFeature
{
    private OutlineRenderPass _renderPass = default;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_renderPass == null) return;

        renderer.EnqueuePass(_renderPass);
    }

    public override void Create()
    {
        _renderPass = new OutlineRenderPass();
    }
}
