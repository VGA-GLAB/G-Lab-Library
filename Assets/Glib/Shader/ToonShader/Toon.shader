Shader "Custom/Toon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_MainColor ("MainColor", Color) = (1, 1, 1, 1)
        _Alpha ("Alpha", Range(0, 1)) = 1
        [Space(20)]
        [Header(Shade1)]
        _Shade1Color ("Color", Color) = (0, 0, 0, 0)
        _Shade1Amount ("Amount", Range(0, 1)) = 0.3
        [Space(10)]
        [Header(Shade2)]
        _Shade2Color ("Color", Color) = (0, 0, 0, 0)
        _Shade2Amount ("Amount", Range(0, 1)) = 0.3
        [Space(20)]
        [Header(Outline)]
        [HDR]_OutlineColor ("OutlineColor", Color) = (0, 0, 0, 1)
        _OutlineRange ("Outline", Float) = 0.0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Library/ToonInput.hlsl"
        ENDHLSL

        // Outlineを描画するPass
        Pass
        {
            Tags { "LightMode" = "ToonOutline"}
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // 奥行きのFogのキーワード定義
            #pragma multi_compile_fog
            
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float fogFactor : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                // 頂点を法線方向に押し出す
                v.vertex += float4(v.normal * _OutlineRange, 0);

                o.vertex = TransformObjectToHClip(v.vertex);
                o.fogFactor = ComputeFogFactor(o.vertex.z);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = _OutlineColor;
                col.a *= _Alpha;
                col.rgb = MixFog(_OutlineColor.rgb, i.fogFactor);

                col.rgb *= col.a;
                return col;
            }
            ENDHLSL
        }

        // モデル自体を描画するPass
        Pass
        {
            Tags{ "LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // 奥行きのFogのキーワード定義
            #pragma multi_compile_fog
            // GPUInstancingに対応させる
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Shader/Library/VertLighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float fogFactor : TEXCOORD1;
                float3 vertexWS : TEXCOORD2;
                float4 vertLight : TEXCOORD3;
                float3 normal : NORMAL;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.fogFactor = ComputeFogFactor(o.vertex.z);
                o.vertexWS = TransformObjectToWorld(v.vertex.xyz);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.vertLight = VertLighting(o.vertexWS, o.normal);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _MainColor;

                Light mainLight = GetMainLight();
                float dotValue = dot(i.normal, mainLight.direction) + saturate(i.vertLight.w) * mainLight.shadowAttenuation;

                // 一影
                if (dotValue >= _Shade1Amount)
                {
                    col.rgb *= mainLight.color;
                }
                else
                {
                    // 二影
                    if (dotValue / _Shade1Amount > _Shade2Amount)
                    {
                        col.rgb *= _Shade1Color.rgb;
                    }
                    else
                    {
                        col.rgb *= _Shade2Color.rgb;
                    }
                }

                col.rgb += i.vertLight.rgb;
                col.a *= _Alpha; 
                col.rgb = MixFog(col.rgb, i.fogFactor);

                col.rgb *= col.a;
                return col;
            }
            ENDHLSL
        }

        UsePass "Hidden/ToonShadow/ShadowCaster"
    }
}
