Shader "Demon/Shadow/Flowing Ink"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Ink noise", 2D) = "white" {}
        _Color ("Ink color", Color) = (0.025,0.022,0.03,0.92)
        _Speed ("Flow speed", Range(-2,2)) = 0.22
        _Density ("Ink density", Range(0,1)) = 0.58
        _WaterLevel ("Submerged cutoff (world Y)", Float) = -10000
        _Drift ("Tip sway (fraction of ribbon width)", Range(0,1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Speed, _Density, _Drift, _WaterLevel;
            CBUFFER_END
            struct Attributes { float4 p:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 p:SV_POSITION; float2 uv:TEXCOORD0; float phase:TEXCOORD1; float worldY:TEXCOORD2; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                float3 anchor = TransformObjectToWorld(float3(0,0,0));
                float phase = dot(anchor, float3(2.71,4.21,1.61));
                float t = _Time.y * _Speed;
                float tip = v.uv.y;
                v.p.x += _Drift * tip * tip * (sin(4*t + phase + tip*3) + 0.3*sin(7*t+phase));
                o.p = TransformObjectToHClip(v.p.xyz);
                o.uv = v.uv;
                o.phase = phase;
                o.worldY = TransformObjectToWorld(v.p.xyz).y;
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                clip(i.worldY - _WaterLevel);
                float2 uv = i.uv;
                float2 scroll = float2(i.phase*0.13, -_Time.y*_Speed+i.phase);
                half n = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv*float2(0.72,0.86)+scroll).r;
                float across = abs(2*uv.x-1);
                float envelope = (1-smoothstep(0.38,1,across)) * smoothstep(0,0.1,uv.y) * (1-smoothstep(0.42,1,uv.y));
                float alpha = smoothstep(0.05,0.52,n + _Density - 0.6) * envelope;
                return half4(_Color.rgb,alpha*_Color.a);
            }
            ENDHLSL
        }
    }
}
