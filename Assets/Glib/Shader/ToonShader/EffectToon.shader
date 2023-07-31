Shader "Custom/EffectToon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_MainColor ("MainColor", Color) = (1, 1, 1, 1)
        _Alpha ("Alpha", Range(0, 1)) = 1
        [Space(20)]
        [Header(Enum)]
        [KeywordEnum(DISSOLVE, GEOMETRY, SLICE)]
        _Effect ("Effect Enum", Int) = 0
        _Amount ("Amount", Range(0, 1)) = 0
        [Space(20)]
        [Header(Tessellation)]
        _TessFactor ("TessFactor", Float) = 10
        _InsideTessFactor ("InsideTessFactor", Float) = 10
        _BreakStrength ("BreakStrength", Float) = 10
        [Space(20)]
        [Header(Dissolve)]
        _DissolveTex ("DissolveTex", 2D) = "white" {}
        _DissolveRange ("Range", Range(0, 1)) = 0
        [HDR] _DissolveColor ("DissolveColor", Color) = (0, 0, 0, 0)
        [Header(Slice)]
        _SliceStrength ("Strength", Float) = 5
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
        _OutlineRange ("Outline", Float) = 0.01
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

        // Outlineを描画するPass
        Pass
        {
            Tags { "LightMode" = "ToonOutline"}
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma geometry geom
            #pragma fragment frag
            // 奥行きのFogのキーワード定義
            #pragma multi_compile_fog

            #pragma multi_compile _ _EFFECT_DISSOLVE _EFFECT_GEOMETRY _EFFECT_SLICE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Library/EffectToonInput.hlsl"
            #include "Library/Random.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2h
            {
                float4 vertex : POS;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct HsControlPointOut
            {
                float3 vertex : POS;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct HsConstantOut
            {
                float tessFactor[3] : SV_TessFactor;
                float insideTessFactor : SV_InsideTessFactor;
            };

            struct d2g
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexWS : TEXCOORD1;
                float normal : NORMAL;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float fogFactor : TEXCOORD1;

                #ifdef _EFFECT_DISSOLVE
                float2 duv : TEXCOORD2;
                #elif _EFFECT_SLICE
                float3 vertexWS : TEXCOORD3;
                #endif
            };

            v2h vert (appdata v)
            {
                v2h o;

                o.vertex = v.vertex;
                o.uv = v.uv;
                o.normal = v.normal;

                return o;
            }

            [domain("tri")]
            [partitioning("integer")]
            [outputtopology("triangle_cw")]
            [patchconstantfunc("hullConst")]
            [outputcontrolpoints(3)]
            HsControlPointOut hull (InputPatch<v2h, 3> i, uint id : SV_OutputControlPointID)
            {
                HsControlPointOut o;
                
                o.vertex = i[id].vertex.xyz;
                o.uv = i[id].uv;
                o.normal = i[id].normal;
                
                return o;
            }

            HsConstantOut hullConst (InputPatch<v2h, 3> i)
            {
                HsConstantOut o;

                #ifdef _EFFECT_GEOMETRY

                o.tessFactor[0] = _TessFactor;
                o.tessFactor[1] = _TessFactor;
                o.tessFactor[2] = _TessFactor;

                o.insideTessFactor = _InsideTessFactor;

                #else

                o.tessFactor[0] = 1;
                o.tessFactor[1] = 1;
                o.tessFactor[2] = 1;

                o.insideTessFactor = 1;

                #endif

                return o;
            }

            [domain("tri")]
            d2g domain (
                HsConstantOut hsConst,
                const OutputPatch<HsControlPointOut, 3> i,
                float3 bary : SV_DomainLocation)
            {
                d2g o;

                float3 positionOS =
                    bary.x * i[0].vertex +
                    bary.y * i[1].vertex +
                    bary.z * i[2].vertex;

                o.uv = 
                    bary.x * i[0].uv +
                    bary.y * i[1].uv + 
                    bary.z * i[2].uv;

                float3 normal =
                    bary.x * i[0].normal +
                    bary.y * i[1].normal + 
                    bary.z * i[2].normal;
                
                positionOS += normal * _OutlineRange;

                o.vertex = TransformObjectToHClip(positionOS);
                o.vertexWS = TransformObjectToWorld(positionOS);
                o.normal = TransformObjectToWorldNormal(normal);
                
                return o;
            }

            [maxvertexcount(3)]
            void geom (triangle d2g input[3], inout TriangleStream<g2f> outStream)
            {
                float3 vec1 = input[1].vertexWS - input[0].vertexWS;
                float3 vec2 = input[2].vertexWS - input[0].vertexWS;
                float3 poriNormal = normalize(cross(vec1, vec2));

                float3 center = (input[0].vertex + input[1].vertex + input[2].vertex) / 3;

                float r = rand(center.xy);

                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    g2f o;
                    float3 dir = 0;

                    #ifdef _EFFECT_GEOMETRY
                    
                    dir = poriNormal * saturate(_Amount * 2 - 1) * _BreakStrength * r;

                    #elif _EFFECT_DISSOLVE

                    o.duv = TRANSFORM_TEX(input[i].uv, _DissolveTex);

                    #elif _EFFECT_SLICE

                    o.vertexWS = input[i].vertexWS;

                    #endif

                    o.vertex = TransformWorldToHClip(input[i].vertexWS + dir);
                    o.fogFactor = ComputeFogFactor(o.vertex.xyz);
 
                    outStream.Append(o);
                }
            }

            half4 frag (g2f i) : SV_Target
            {
                #ifdef _EFFECT_SLICE

                if (frac((i.vertexWS.y) * _SliceStrength) - _Amount < 0)
                {
                    discard;
                }
                
                #endif

                half4 col = _OutlineColor;

                #ifdef _EFFECT_DISSOLVE

                float dAlpha = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, i.duv).r;
                if (dAlpha < _Amount)
                {
                    col.a = 0;
                }

                #endif

                col.a *= _Alpha;
                col.rgb = MixFog(_OutlineColor.rgb, i.fogFactor);
                
                #ifdef _EFFECT_GEOMETRY

                col.rgb = lerp(col.rgb, Mono(col.rgb), saturate(_Amount * 2));

                #endif

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
            #pragma hull hull
            #pragma domain domain
            #pragma geometry geom
            #pragma fragment frag
            // 奥行きのFogのキーワード定義
            #pragma multi_compile_fog
            // GPUInstancingに対応させる
            #pragma multi_compile_instancing
            
            #pragma multi_compile _ _EFFECT_DISSOLVE _EFFECT_GEOMETRY _EFFECT_SLICE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Library/EffectToonInput.hlsl"
            #include "Library/Random.hlsl"
            #include "Library/VertLighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2h
            {
                float4 vertex : POS;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct HsControlPointOut
            {
                float3 vertex : POS;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct HsConstantOut
            {
                float tessFactor[3] : SV_TessFactor;
                float insideTessFactor : SV_InsideTessFactor;
            };

            struct d2g
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexWS : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float fogFactor : TEXCOORD1;
                float3 vertexWS : TEXCOORD2;
                float2 duv : TEXCOORD3;
                float4 vertLight : TEXCOORD4;
                float3 normal : NORMAL;
            };

            v2h vert (appdata v)
            {
                v2h o;

                o.vertex = v.vertex;
                o.uv = v.uv;
                o.normal = v.normal;

                return o;
            }

            [domain("tri")]
            [partitioning("integer")]
            [outputtopology("triangle_cw")]
            [patchconstantfunc("hullConst")]
            [outputcontrolpoints(3)]
            HsControlPointOut hull (InputPatch<v2h, 3> i, uint id : SV_OutputControlPointID)
            {
                HsControlPointOut o;
                
                o.vertex = i[id].vertex.xyz;
                o.uv = i[id].uv;
                o.normal = i[id].normal;
                
                return o;
            }

            HsConstantOut hullConst (InputPatch<v2h, 3> i)
            {
                HsConstantOut o;

                #ifdef _EFFECT_GEOMETRY

                o.tessFactor[0] = _TessFactor;
                o.tessFactor[1] = _TessFactor;
                o.tessFactor[2] = _TessFactor;

                o.insideTessFactor = _InsideTessFactor;

                #else

                o.tessFactor[0] = 1;
                o.tessFactor[1] = 1;
                o.tessFactor[2] = 1;

                o.insideTessFactor = 1;

                #endif

                return o;
            }

            [domain("tri")]
            d2g domain (
                HsConstantOut hsConst,
                const OutputPatch<HsControlPointOut, 3> i,
                float3 bary : SV_DomainLocation)
            {
                d2g o;

                float3 positionOS =
                    bary.x * i[0].vertex +
                    bary.y * i[1].vertex +
                    bary.z * i[2].vertex;

                o.uv = 
                    bary.x * i[0].uv +
                    bary.y * i[1].uv + 
                    bary.z * i[2].uv;

                float3 normal =
                    bary.x * i[0].normal +
                    bary.y * i[1].normal + 
                    bary.z * i[2].normal;

                o.vertexWS = TransformObjectToWorld(positionOS);
                o.vertex = TransformObjectToHClip(positionOS);
                o.normal = TransformObjectToWorldNormal(normal);
                
                return o;
            }

            [maxvertexcount(3)]
            void geom (triangle d2g input[3], inout TriangleStream<g2f> outStream)
            {
                float3 vec1 = input[1].vertexWS - input[0].vertexWS;
                float3 vec2 = input[2].vertexWS - input[0].vertexWS;
                float3 poriNormal = normalize(cross(vec1, vec2));

                float3 center = (input[0].vertex + input[1].vertex + input[2].vertex) / 3;

                float r = rand(center.xy);

                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    g2f o;
                    float3 dir = 0;

                    #ifdef _EFFECT_GEOMETRY
                    
                    dir = poriNormal * saturate(_Amount * 2 - 1) * _BreakStrength * r;

                    #endif
                    o.vertex = TransformWorldToHClip(input[i].vertexWS + dir);
                    o.vertexWS = input[i].vertexWS;
                    o.normal = input[i].normal;
                    o.vertLight = VertLighting(o.vertexWS, o.normal);
                    o.uv = TRANSFORM_TEX(input[i].uv, _MainTex);
                    o.duv = TRANSFORM_TEX(input[i].uv, _DissolveTex);
                    o.fogFactor = ComputeFogFactor(o.vertex.xyz);
 
                    outStream.Append(o);
                }
            }
            half4 frag (g2f i) : SV_Target
            {
                #ifdef _EFFECT_SLICE

                if (frac((i.vertexWS.y) * _SliceStrength) - _Amount < 0)
                {
                    discard;
                }
                
                #endif

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _MainColor;

                Light mainLight = GetMainLight();
                float dotValue = dot(i.normal, mainLight.direction) + saturate(i.vertLight.w);

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

                #ifdef _EFFECT_DISSOLVE

                float dAlpha = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, i.duv);
                float amount = remap(_Amount, -_DissolveRange, 1);
                if (dAlpha < amount + _DissolveRange)
                {
                    col.rgb += _DissolveColor.rgb;

                    if (dAlpha < amount)
                    {
                        col.a = 0;
                    }
                }

                #endif

                Light addLight = GetAdditionalLight(0, i.vertexWS);

                col.rgb += i.vertLight.rgb;
                col.a *= _Alpha; 
                col.rgb = MixFog(col.rgb, i.fogFactor);

                #ifdef _EFFECT_GEOMETRY

                col.rgb = lerp(col.rgb, Mono(col.rgb), saturate(_Amount * 2));

                #endif

                col.rgb *= col.a;
                return col;
            }
            ENDHLSL
        }

        UsePass "Hidden/ToonShadow/ShadowCaster"
    }
}
