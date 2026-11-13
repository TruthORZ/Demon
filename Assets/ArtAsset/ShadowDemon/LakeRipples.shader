Shader "Demon/Shadow/Lake Ripples"
{
    Properties
    {
        _WaterLevel ("Lake surface height (world Y)", Float) = 95.4
        _Tint ("Ripple colour", Color) = (.46,.62,.68,.42)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+8" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _WaterLevel;
                half4 _Tint;
            CBUFFER_END
            struct Input { float4 position:POSITION; float2 uv:TEXCOORD0; };
            struct Output { float4 position:SV_POSITION; float2 uv:TEXCOORD0; half fog:TEXCOORD1; };
            Output Vert(Input v)
            {
                Output o;
                float3 world = TransformObjectToWorld(v.position.xyz);
                world.y = _WaterLevel + .025;
                o.position = TransformWorldToHClip(world);
                o.uv = 2*v.uv - 1;
                o.fog = ComputeFogFactor(o.position.z);
                return o;
            }
            half4 Frag(Output i):SV_Target
            {
                float angle = atan2(i.uv.y,i.uv.x);
                float r = length(i.uv) + .018*sin(5*angle+_Time.y*.4);
                float phase = frac(2.6*r-_Time.y*.16);
                float crest = 1 - smoothstep(.02,.09,abs(phase-.5));
                float envelope = smoothstep(.16,.38,r)*(1-smoothstep(.57,1,r));
                return half4(MixFog(_Tint.rgb,i.fog),_Tint.a*crest*envelope);
            }
            ENDHLSL
        }
    }
}
