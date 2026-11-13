Shader "Demon/Shadow/Lake Silhouette"
{
    Properties
    {
        _BaseColor ("Shadow colour", Color) = (.06,.059,.067,1)
        _WaterLevel ("Lake surface height (world Y)", Float) = 95.4
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _WaterLevel;
            CBUFFER_END
            struct Input { float4 position : POSITION; float3 normal : NORMAL; };
            struct Output { float4 position : SV_POSITION; float3 world : TEXCOORD0; half3 normal : TEXCOORD1; half fog : TEXCOORD2; };
            Output Vert(Input v)
            {
                Output o;
                o.world = TransformObjectToWorld(v.position.xyz);
                o.position = TransformWorldToHClip(o.world);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.fog = ComputeFogFactor(o.position.z);
                return o;
            }
            half4 Frag(Output i) : SV_Target
            {
                clip(i.world.y - _WaterLevel);
                Light sun = GetMainLight();
                half3 n = normalize(i.normal);
                half3 color = _BaseColor.rgb * (SampleSH(n) + sun.color * saturate(dot(n, sun.direction)));
                return half4(MixFog(color, i.fog), 1);
            }
            ENDHLSL
        }
    }
}
