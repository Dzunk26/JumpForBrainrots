// shader particle chung cho VFX brainrot: texture (flipbook qua Texture Sheet Animation) x mau HDR x vertex color
// mau HDR dat o material vi vertex color cua particle bi clamp 0..1
// bat Erosion: alpha cua vertex color lam nguong an mon texture -> hinh mong dan roi tan, khong bi mo deu
Shader "_Game/VFX/Brainrot Particle" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _AlphaBoost ("Alpha Boost", Range(0, 4)) = 1

        [Header(Erosion)]
        [ToggleUI] _IsErosion ("Is Erosion (Vertex Alpha = Threshold)", Float) = 0
        _ErosionSoftness ("Erosion Softness", Range(0.01, 1)) = 0.2

        [Header(Blend)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend (One = Additive, OneMinusSrcAlpha = Alpha)", Float) = 1
    }

    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "PreviewType" = "Plane" }

        Pass {
            Name "BrainrotParticle"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _AlphaBoost;
                half _IsErosion;
                half _ErosionSoftness;
            CBUFFER_END

            struct Attributes {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input) {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                half fadeAlpha = tex.a * input.color.a;
                half erosionAlpha = saturate((tex.a - (1.0 - input.color.a)) / _ErosionSoftness);
                half alpha = saturate(lerp(fadeAlpha, erosionAlpha, _IsErosion) * _Color.a * _AlphaBoost);

                half3 color = tex.rgb * input.color.rgb * _Color.rgb;

                // output premultiplied: dung duoc ca additive (One One) lan alpha (One OneMinusSrcAlpha)
                return half4(color * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
