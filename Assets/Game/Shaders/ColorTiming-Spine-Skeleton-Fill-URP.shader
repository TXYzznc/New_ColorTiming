// Compatible replacement for Spine/Skeleton Fill when the project uses URP.
// Preserves the original Spine 3.8 material property contract used by Boss hit feedback.
Shader "ColorTiming/URP/Spine Skeleton Fill"
{
    Properties
    {
        _FillColor("Fill Color", Color) = (1, 1, 1, 1)
        _FillPhase("Fill Phase", Range(0, 1)) = 0
        [NoScaleOffset] _MainTex("Main Texture", 2D) = "white" {}
        [Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
        [HideInInspector] _StencilRef("Stencil Reference", Float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilComp]
            Pass Keep
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _ _STRAIGHT_ALPHA_INPUT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _FillColor;
                half _FillPhase;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 rawColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half finalAlpha = rawColor.a * input.color.a;

                #if defined(_STRAIGHT_ALPHA_INPUT)
                    rawColor.rgb *= rawColor.a;
                #endif

                half3 texturedColor = rawColor.rgb * input.color.rgb;
                half3 fillColor = _FillColor.rgb * finalAlpha;
                return half4(lerp(texturedColor, fillColor, _FillPhase), finalAlpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
