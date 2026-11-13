Shader "Demon/Shadow/Face Emission"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Eye and tooth mask", 2D) = "black" {}
        [HDR] _Tint ("Glow color", Color) = (1,1,1,1)
        _Intensity ("Brightness", Range(0,8)) = 1.5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+1" "RenderType"="Transparent" }
        Pass
        {
            Blend One One
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                half _Intensity;
            CBUFFER_END
            struct Input { float4 p:POSITION; float2 uv:TEXCOORD0; };
            struct Output { float4 p:SV_POSITION; float2 uv:TEXCOORD0; };
            Output Vert(Input v)
            {
                Output o;
                o.p = TransformObjectToHClip(v.p.xyz);
                o.uv = v.uv;
                return o;
            }
            half4 Frag(Output i):SV_Target
            {
                half value = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).r;
                value = saturate((value - 0.035h) / 0.965h);
                return half4(value * _Tint.rgb * _Intensity,0);
            }
            ENDHLSL
        }
    }
}

