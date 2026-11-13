Shader "Demon/Solar Disc"
{
    Properties { [HDR] _BaseColor ("Disc color", Color)=(1,1,1,1) }
    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-100" "RenderType"="Transparent" }
        Pass {
            Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Input { float4 pos:POSITION; float2 uv:TEXCOORD0; };
            struct Output {float4 pos:SV_POSITION; float2 uv:TEXCOORD0;};
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END
            Output Vert(Input v){ Output o;o.pos=TransformObjectToHClip(v.pos.xyz);o.uv=v.uv;return o; }
            half4 Frag(Output i):SV_Target {
                float r=length(i.uv-.5);
                float body=1-smoothstep(.305,.317,r);
                float halo=exp(-max(0,r-.31)*19)*(1-smoothstep(.44,.5,r))*.55;
                return half4(_BaseColor.rgb,max(body,halo)*_BaseColor.a);
            }
            ENDHLSL
        }
    }
}

