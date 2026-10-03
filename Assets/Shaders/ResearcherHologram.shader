Shader "NullSignal/Researcher Hologram"
{
    Properties
    {
        _BaseColor ("Recorded surface", Color) = (0.8,0.82,0.77,1)
        _TintColor ("Projection tint", Color) = (0.32,0.77,0.82,1)
        _Opacity ("Opacity", Range(0,1)) = 0.7
        _SignalStrength ("Signal strength", Range(0,1.5)) = 1
        _Distortion ("Distortion", Range(0,0.05)) = 0.012
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Hologram"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _TintColor;
                float _Opacity;
                float _SignalStrength;
                float _Distortion;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                // One narrow rolling dropout, not a constant full-body glitch.
                float band = pow(saturate(1.0 - abs(frac(world.y * .43 - _Time.y * .21) - .5) * 28.0), 3.0);
                world.x += sin(world.y * 35.0 + _Time.y * 17.0) * _Distortion * band;
                output.positionWS = world;
                output.positionCS = TransformWorldToHClip(world);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half3 normal = normalize(input.normalWS);
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half rim = pow(1.0h - saturate(abs(dot(normal, view))), 2.5h);
                half scan = .83h + .17h * sin(input.positionWS.y * 105.0 - _Time.y * 9.0);
                half sweep = pow(saturate(1.0 - abs(frac(input.positionWS.y * .24 - _Time.y * .13) - .5) * 10.0), 3.0);
                half shade = .54h + .46h * saturate(dot(normal, normalize(half3(.3h, .8h, -.5h))));
                half3 color = _BaseColor.rgb * shade * scan + _TintColor.rgb * (.13h + rim * .72h + sweep * .25h);
                half alpha = saturate(_Opacity * (.82h + .18h * scan) * _SignalStrength + rim * .12h);
                return half4(color * (.88h + .12h * _SignalStrength), alpha);
            }
            ENDHLSL
        }
    }
}
