Shader "Rollfaehren Fury/URP/Scrolling River Water"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (0.06, 0.42, 0.65, 0.72)
        _FoamColor ("Foam Color", Color) = (0.85, 0.98, 1.0, 1.0)
        [MainTexture] _WaveTex ("Wave Texture", 2D) = "white" {}
        _WaveTiling ("Wave Tiling", Float) = 12
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.45
        _Alpha ("Alpha", Range(0, 1)) = 0.72
        _SpeedA ("Speed A", Vector) = (0.035, 0.018, 0, 0)
        _SpeedB ("Speed B", Vector) = (-0.018, 0.026, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _FoamColor;
                float4 _WaveTex_ST;
                float4 _SpeedA;
                float4 _SpeedB;
                float _WaveTiling;
                float _WaveStrength;
                float _Alpha;
            CBUFFER_END

            TEXTURE2D(_WaveTex);
            SAMPLER(sampler_WaveTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _WaveTex);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                float2 uv = input.uv * max(_WaveTiling, 0.001);

                float waveA = SAMPLE_TEXTURE2D(_WaveTex, sampler_WaveTex, uv + time * _SpeedA.xy).r;
                float waveB = SAMPLE_TEXTURE2D(_WaveTex, sampler_WaveTex, uv * 1.37 + time * _SpeedB.xy).g;
                float wave = saturate((waveA + waveB) * 0.5);
                float foam = smoothstep(0.62, 0.98, wave) * _WaveStrength;

                float3 color = lerp(_BaseColor.rgb, _FoamColor.rgb, foam);
                float alpha = saturate(_Alpha + foam * 0.18);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
