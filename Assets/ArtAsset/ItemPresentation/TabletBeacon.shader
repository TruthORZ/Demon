Shader "Demon/Tablet Beacon"
{
    Properties { [HDR] _Color ("Light Color", Color) = (1.5, 1.15, 0.55, 0.66) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Input { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Output { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; float h:TEXCOORD2; };
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END
            Output Vert(Input v)
            {
                Output o;
                o.world = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.world);
                o.normal = TransformObjectToWorldNormal(v.normalOS);
                o.h = v.uv.y;
                return o;
            }
            half4 Frag(Output i):SV_Target
            {
                half center = saturate(dot(normalize(i.normal), GetWorldSpaceNormalizeViewDir(i.world)));
                half fade = 1.0 - smoothstep(0.55, 1.0, i.h);
                half nearFade = smoothstep(2.5, 7.0, distance(_WorldSpaceCameraPos, i.world));
                return half4(_Color.rgb, _Color.a * center * center * fade * nearFade);
            }
            ENDHLSL
        }
    }
}
