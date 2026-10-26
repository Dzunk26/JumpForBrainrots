// shader duong mui ten huong dan: ve chevron bang toan hoc (khong can texture)
// mau: 1 dai cau vong (hue HSV) lien mach keo gian tu start den target, ca dai troi lien tuc ve phia target
// dung voi plane cua TutorialPathArrow: uv.x 0..1 doc chieu dai, uv.y chay ngang be rong
// _Length va _Width (met) do script set qua MaterialPropertyBlock
// moi kich thuoc tinh bang met -> doi spacing / do day / be rong khong lam meo hinh mui ten
Shader "_Game/VFX/Tutorial Path Arrow" {
    Properties {
        [Header(Shape (m))]
        _Spacing ("Spacing", Float) = 1.4
        _ArrowDepth ("Arrow Depth (tip to arm end)", Float) = 0.55
        _ArrowThickness ("Arrow Thickness", Float) = 0.4
        _ArmSpan ("Arm Span (ratio of width)", Range(0.3, 1)) = 0.85
        _OutlineWidth ("White Outline Width", Float) = 0.06

        [Header(Animation)]
        _ScrollSpeed ("Scroll Speed (m/s)", Float) = 2

        [Header(Color)]
        _RainbowCycles ("Rainbow Cycles On Path", Float) = 1
        _RainbowSpeed ("Rainbow Speed (cycles/s)", Float) = 0.375
        _Saturation ("Saturation", Range(0, 1)) = 0.6
        _Value ("Value", Range(0, 1)) = 1
        _Intensity ("Intensity", Range(0.5, 4)) = 1
        _Alpha ("Alpha", Range(0, 1)) = 0.95
        _FadeIn ("Fade In (m)", Float) = 1
        _FadeOut ("Fade Out (m)", Float) = 1

        [Header(Set By Script)]
        _Length ("Length", Float) = 10
        _Width ("Width", Float) = 1.6
    }

    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass {
            Name "TutorialPathArrow"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Offset -1, -1

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Spacing;
                float _ArrowDepth;
                float _ArrowThickness;
                float _ArmSpan;
                float _OutlineWidth;
                float _ScrollSpeed;
                float _RainbowCycles;
                float _RainbowSpeed;
                half _Saturation;
                half _Value;
                half _Intensity;
                half _Alpha;
                float _FadeIn;
                float _FadeOut;
                float _Length;
                float _Width;
            CBUFFER_END

            float3 HsvToRgb(float hue, float saturation, float value) {
                float3 k = saturate(abs(frac(hue + float3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0);
                return value * lerp(float3(1.0, 1.0, 1.0), k, saturation);
            }

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
                float pathDistance = input.uv.x * _Length;

                // tru thoi gian -> chevron chay ve phia cuoi duong (target)
                float scroll = (pathDistance - _Time.y * _ScrollSpeed) / _Spacing;
                float cellX = frac(scroll) * _Spacing;
                float acrossY = abs(input.uv.y - 0.5) * _Width;

                // 2 canh chevron: duong thang tu mui (giua o) lui ve sau toi dau canh
                // chia sqrt de do day do vuong goc voi canh -> canh day deu, khong bi meo
                float armSpan = _Width * 0.5 * _ArmSpan;
                float slope = _ArrowDepth / armSpan;
                float tipX = (_Spacing + _ArrowDepth) * 0.5;
                float signedArmDistance = (cellX - tipX + acrossY * slope) * rsqrt(1.0 + slope * slope);
                float shapeDistance = max(abs(signedArmDistance) - _ArrowThickness * 0.5, acrossY - armSpan);

                float aa = max(fwidth(shapeDistance), 0.0001);
                float body = 1.0 - smoothstep(-aa, aa, shapeDistance);
                float outline = 1.0 - smoothstep(_OutlineWidth - aa, _OutlineWidth + aa, shapeDistance);

                // hue theo ti le vi tri tren duong (0 = start, 1 = target) -> 1 dai cau vong trai het duong du dai hay ngan
                // tru thoi gian -> ca dai troi ve phia target
                float hue = input.uv.x * _RainbowCycles - _Time.y * _RainbowSpeed;
                half3 rainbow = HsvToRgb(frac(hue), _Saturation, _Value) * _Intensity;

                half3 color = lerp(half3(1, 1, 1), rainbow, body);
                float fade = smoothstep(0.0, _FadeIn, pathDistance) * smoothstep(_Length, _Length - _FadeOut, pathDistance);
                half alpha = outline * fade * _Alpha;

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
