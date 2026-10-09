// shader cho than loc xoay: vet xoan oc cuon len + noise + fresnel vien sang
// mesh can uv.x = goc quanh truc (0..1), uv.y = chieu cao (0 day, 1 dinh)
Shader "_Game/VFX/Tornado Swirl" {
    Properties {
        [HDR] _CoreColor ("Core Color", Color) = (1.2, 2.6, 4, 1)
        [HDR] _EdgeColor ("Edge Color", Color) = (0.15, 0.55, 2.4, 1)
        _Alpha ("Alpha", Range(0, 2)) = 0.8

        [Header(Bands)]
        _BandCount ("Band Count", Float) = 3
        _BandTilt ("Band Tilt", Float) = 1.2
        _BandSharpness ("Band Sharpness", Range(0.5, 8)) = 2.5

        [Header(Noise)]
        _NoiseScale ("Noise Scale (X Around, Y Up)", Vector) = (8, 3, 0, 0)
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.6

        [Header(Motion)]
        _SwirlSpeed ("Swirl Speed", Float) = 0.55
        _RiseSpeed ("Rise Speed", Float) = 0.35
        _WobbleAmount ("Wobble Amount", Float) = 0.15
        _WobbleFrequency ("Wobble Frequency", Float) = 1.2
        _WobbleSpeed ("Wobble Speed", Float) = 1.8

        [Header(Shape)]
        _FresnelPower ("Fresnel Power", Range(0.1, 8)) = 2
        _FresnelBoost ("Fresnel Boost", Range(0, 4)) = 1.4
        _BottomFade ("Bottom Fade", Range(0.01, 1)) = 0.06
        _TopFade ("Top Fade", Range(0.01, 1)) = 0.35
        _BaseGlow ("Base Glow", Range(0, 4)) = 1.2

        [Header(Blend)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend (One = Additive, OneMinusSrcAlpha = Alpha)", Float) = 1
    }

    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass {
            Name "TornadoSwirl"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _EdgeColor;
                half _Alpha;
                float _BandCount;
                float _BandTilt;
                half _BandSharpness;
                float4 _NoiseScale;
                half _NoiseStrength;
                float _SwirlSpeed;
                float _RiseSpeed;
                float _WobbleAmount;
                float _WobbleFrequency;
                float _WobbleSpeed;
                half _FresnelPower;
                half _FresnelBoost;
                half _BottomFade;
                half _TopFade;
                half _BaseGlow;
            CBUFFER_END

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            float Hash(float2 p) {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // wrap theo truc x de noise lap lai quanh vong tron, khong bi duong noi
            float WrapX(float x, float period) {
                return x - period * floor(x / period);
            }

            float ValueNoise(float2 p, float period) {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float x0 = WrapX(i.x, period);
                float x1 = WrapX(i.x + 1.0, period);

                float a = Hash(float2(x0, i.y));
                float b = Hash(float2(x1, i.y));
                float c = Hash(float2(x0, i.y + 1.0));
                float d = Hash(float2(x1, i.y + 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float Fbm(float2 p, float period) {
                float value = 0.0;
                float amplitude = 0.5;

                for (int i = 0; i < 3; i++) {
                    value += ValueNoise(p, period) * amplitude;
                    p *= 2.0;
                    period *= 2.0;
                    amplitude *= 0.5;
                }

                return value / 0.875;
            }

            Varyings Vert(Attributes input) {
                Varyings output;

                // uon than loc, cang len cao cang lac manh
                float3 positionOS = input.positionOS.xyz;
                float height01 = input.uv.y;
                float wobbleTime = _Time.y * _WobbleSpeed;
                float2 wobble = float2(
                    sin(height01 * _WobbleFrequency * 6.2831853 + wobbleTime),
                    cos(height01 * _WobbleFrequency * 5.1 + wobbleTime * 1.3));
                positionOS.xz += wobble * _WobbleAmount * height01;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                output.positionCS = positionInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                output.uv = input.uv;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target {
                float time = _Time.y;
                float2 uv = input.uv;

                float period = max(1.0, round(_NoiseScale.x));
                float2 noiseUV = float2(
                    (uv.x + time * _SwirlSpeed + uv.y * _BandTilt * 0.5) * period,
                    (uv.y - time * _RiseSpeed) * _NoiseScale.y);
                float noise = Fbm(noiseUV, period);

                // vet xoan oc, bi noise lam meo cho giong gio
                float bandCount = max(1.0, round(_BandCount));
                float phase = (uv.x + time * _SwirlSpeed) * bandCount + uv.y * _BandTilt + (noise - 0.5) * _NoiseStrength * 2.0;
                float band = 0.5 + 0.5 * sin(phase * 6.2831853);
                band = pow(saturate(band), _BandSharpness);
                float streak = saturate(band * lerp(1.0, noise * 1.6, _NoiseStrength));

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float fresnel = pow(1.0 - abs(dot(normalWS, viewDirWS)), _FresnelPower);

                float fade = smoothstep(0.0, _BottomFade, uv.y) * smoothstep(1.0, 1.0 - _TopFade, uv.y);
                float baseGlow = pow(1.0 - uv.y, 6.0) * _BaseGlow;

                float mask = (streak * (0.35 + fresnel * _FresnelBoost) + baseGlow * (0.5 + streak)) * fade * _Alpha;
                float3 color = lerp(_EdgeColor.rgb, _CoreColor.rgb, saturate(streak * 0.6 + baseGlow));

                // output premultiplied: dung duoc ca additive (One One) lan alpha (One OneMinusSrcAlpha)
                return half4(color * mask, saturate(mask));
            }
            ENDHLSL
        }
    }
}
