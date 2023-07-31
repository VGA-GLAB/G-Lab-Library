using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRenderPass : ScriptableRenderPass
{
    /// <summary>Outlineを描画するために使用するPassのLightMode</summary>
    private const string OUTLINE_LIGHTMODETAG = "ToonOutline";
    /// <summary>レンダリングするタイミング</summary>
    private readonly RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    /// <summary>対象のRenderQueue</summary>
    private readonly RenderQueueRange _renderQueueRange = RenderQueueRange.transparent;
    /// <summary>Tagを追加するために必要な構造体</summary>
    private ShaderTagId _tagId = default;

    public OutlineRenderPass()
    {
        renderPassEvent = _renderPassEvent;
        _tagId = new ShaderTagId(OUTLINE_LIGHTMODETAG);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // カメラで描画するタイミングでTagが呼ばれるようにしている
        var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
        var drawingSetting = CreateDrawingSettings(_tagId, ref renderingData, sortFlags);
        var filteringSetting = new FilteringSettings(_renderQueueRange);
        drawingSetting.perObjectData = PerObjectData.None;
        context.DrawRenderers(renderingData.cullResults, ref drawingSetting, ref filteringSetting);
    }
}
