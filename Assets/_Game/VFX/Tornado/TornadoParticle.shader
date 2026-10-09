// shader particle cho loc xoay: cham sang mem hoac vong tron, khong can texture
// mau HDR dat o material vi vertex color cua particle bi clamp 0..1
Shader "_Game/VFX/Tornado Particle" {
    Properties {
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        [ToggleUI] _IsRing ("Is Ring", Float) = 0
        _RingRadius ("Ring Radius", Range(0, 1)) = 0.8
        _RingWidth ("Ring Width", Range(0.01, 1)) = 0.12
        _Softness ("Softness", Range(0.5, 8)) = 2

        [Header(Blend)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend (One = Additive, OneMinusSrcAlpha = Alpha)", Float) = 1
    }

    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "PreviewType" = "Plane" }

        Pass {
            Name "TornadoParticle"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _IsRing;
                half _RingRadius;
                half _RingWidth;
                half _Softness;
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
                output.uv = input.uv;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target {
                float centerDistance = length(input.uv - 0.5) * 2.0;
                float dotShape = pow(saturate(1.0 - centerDistance), _Softness);
                float ringShape = pow(saturate(1.0 - abs(centerDistance - _RingRadius) / _RingWidth), _Softness);
                float shape = lerp(dotShape, ringShape, _IsRing);

                half4 color = _Color * input.color;
                float alpha = color.a * shape;

                return half4(color.rgb * alpha, saturate(alpha));
            }
            ENDHLSL
        }
    }
}
