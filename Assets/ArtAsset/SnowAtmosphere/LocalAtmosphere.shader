Shader "Demon/Local Atmosphere"
{
    Properties {
        [Toggle(_FLASHLIGHT)] _Flashlight ("Flashlight cone", Float) = 0
        _Density ("Density per metre", Range(0,1)) = 0.14
        _Tint ("Scattering tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+30" "RenderType"="Transparent" }
        Pass
        {
            Cull Front ZWrite Off ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _FLASHLIGHT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            struct Input { float4 vertex : POSITION; };
            struct Varyings { float4 pos:SV_POSITION; float3 world:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                float _Density;
                half4 _Tint;
            CBUFFER_END
            Varyings Vert(Input v) {
                Varyings o; o.world = TransformObjectToWorld(v.vertex.xyz); o.pos=TransformWorldToHClip(o.world); return o;
            }
            half4 Frag(Varyings i):SV_Target {
                float3 eye=GetCameraPositionWS(), dir=normalize(i.world-eye);
                float3 p=TransformWorldToObject(eye);
                float3 d=mul((float3x3)GetWorldToObjectMatrix(),dir);
                float3 inv=rcp(lerp(-1.0,1.0,step(0,d))*max(abs(d),0.000001));
                float3 lo=-0.5, hi=0.5;
                #if defined(_FLASHLIGHT)
                lo.z=0; hi.z=1;
                #endif
                float3 a=(lo-p)*inv,b=(hi-p)*inv;
                float3 entry=min(a,b),exitT=max(a,b);
                float start=max(0,max(max(entry.x,entry.y),entry.z));
                float stop=min(min(exitT.x,exitT.y),exitT.z);
                float depth=SampleSceneDepth(GetNormalizedScreenSpaceUV(i.pos));
                float sceneT=LinearEyeDepth(depth,_ZBufferParams)/max(.001,-mul((float3x3)UNITY_MATRIX_V,dir).z);
                stop=min(stop,sceneT);
                float stride=max(0,stop-start)/24;
                float sum=0;
                [unroll] for(int n=0;n<24;n++){
                    float3 q=p+d*(start+(n+.5)*stride);
                    #if defined(_FLASHLIGHT)
                    float profile=saturate(1-dot(q.xy,q.xy)/max(.00001,.25*q.z*q.z));
                    sum+=profile*profile*pow(saturate(1-q.z),2);
                    #else
                    float profile=saturate(1-4*dot(q,q));
                    sum+=profile*profile;
                    #endif
                }
                half opacity=1-exp(-_Density*stride*sum);
                #if defined(_FLASHLIGHT)
                return half4(_Tint.rgb,opacity);
                #else
                return half4(unity_FogColor.rgb*_Tint.rgb,opacity);
                #endif
            }
            ENDHLSL
        }
    }
}
