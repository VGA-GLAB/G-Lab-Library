Shader "Hidden/ToonShadow"
{
    SubShader
    {
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 vert (float4 vert : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(vert.xyz);
            }

            half4 frag (float4 i : SV_POSITION) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
