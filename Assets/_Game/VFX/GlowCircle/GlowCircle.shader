// shader vong tron sang duoi chan base (upgrade, shop,...): vien sang + nen mo + song lan toa
// dung tren quad uv 0..1, mau doi qua _Color (material hoac MaterialPropertyBlock qua GlowCircleVFX)
Shader "_Game/VFX/Glow Circle" {
    Properties {
        [HDR] _Color ("Color", Color) = (0.2, 1.6, 0.9, 1)
        _Alpha ("Alpha", Range(0, 2)) = 1

        [Header(Ring)]
        _RingRadius ("Ring Radius", Range(0, 1)) = 0.9
        _RingWidth ("Ring Width", Range(0.001, 0.3)) = 0.07
        _RingSoftness ("Ring Softness", Range(0.001, 0.2)) = 0.03
        _RingIntensity ("Ring Intensity", Range(0, 4)) = 1.6
        _RingWhiten ("Ring Whiten", Range(0, 1)) = 0.25
        _OuterGlow ("Outer Glow", Range(0, 2)) = 0.6

        [Header(Fill)]
        _FillAlpha ("Fill Alpha", Range(0, 1)) = 0.35
        _FillEdgeBoost ("Fill Edge Boost", Range(0, 2)) = 0.5
        _FillWhiten ("Fill Whiten", Range(0, 1)) = 0.35


        [Header(Animation)]
        _PulseSpeed ("Pulse Speed", Float) = 2.5
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.25
        _WaveSpeed ("Wave Speed", Float) = 0.6
        _WaveWidth ("Wave Width", Range(0.01, 0.5)) = 0.12
        _WaveIntensity ("Wave Intensity", Range(0, 2)) = 0.5

        [Header(Blend)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend (One = Additive, OneMinusSrcAlpha = Alpha)", Float) = 10
    }

    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass {
            Name "GlowCircle"
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
                half _Alpha;
                half _RingRadius;
                half _RingWidth;
                half _RingSoftness;
                half _RingIntensity;
                half _RingWhiten;
                half _OuterGlow;
                half _FillAlpha;
                half _FillEdgeBoost;
                half _FillWhiten;
                float _PulseSpeed;
                half _PulseAmount;
                float _WaveSpeed;
                half _WaveWidth;
                half _WaveIntensity;
            CBUFFER_END

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input) {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target {
                float time = _Time.y;
                float2 p = input.uv * 2.0 - 1.0;
                float r = length(p);

                // vien: dai sang quanh _RingRadius + glow mo dan ra ngoai
                float ringInner = _RingRadius - _RingWidth * 0.5;
                float ringOuter = _RingRadius + _RingWidth * 0.5;
                float ring = smoothstep(ringInner - _RingSoftness, ringInner, r) * (1.0 - smoothstep(ringOuter, ringOuter + _RingSoftness, r));
                float outerGlow = saturate(1.0 - (r - ringOuter) / max(1.0 - ringOuter, 0.001));
                outerGlow = outerGlow * outerGlow * step(ringOuter, r) * _OuterGlow;

                float pulse = 1.0 + sin(time * _PulseSpeed) * _PulseAmount;

                // nen ben trong vien, sang dan ve phia vien
                float inside = 1.0 - smoothstep(ringInner - _RingSoftness, ringInner, r);
                float fill = _FillAlpha * (1.0 + pow(saturate(r / max(ringInner, 0.001)), 3.0) * _FillEdgeBoost);

                // song lan toa tu tam ra vien
                float waveRadius = frac(time * _WaveSpeed) * ringInner;
                float wave = saturate(1.0 - abs(r - waveRadius) / _WaveWidth);
                wave = wave * wave * _WaveIntensity * (1.0 - waveRadius / max(ringInner, 0.001));

                float3 white = float3(1.0, 1.0, 1.0);
                float3 baseColor = _Color.rgb;
                float3 fillColor = lerp(baseColor, white * max(max(baseColor.r, baseColor.g), baseColor.b), _FillWhiten);
                float3 ringColor = lerp(baseColor, white * max(max(baseColor.r, baseColor.g), baseColor.b), _RingWhiten);

                float fillMask = inside * fill;
                float ringMask = ring * _RingIntensity * pulse;
                float glowMask = outerGlow * pulse;
                float waveMask = inside * wave;

                float3 color = fillColor * fillMask + ringColor * ringMask + baseColor * (glowMask + waveMask);
                float alpha = saturate(fillMask + ring + glowMask + waveMask) * _Alpha * _Color.a;

                // output premultiplied: dung duoc ca alpha blend (One OneMinusSrcAlpha) lan additive (One One)
                return half4(color * _Alpha * _Color.a, alpha);
            }
            ENDHLSL
        }
    }
}
