// shader tuong sang quanh vong tron: sang o chan, mo dan len tren tao chieu sau 3D
// dung tren mesh tru ho (uv.x = goc quanh truc, uv.y = chieu cao 0 day -> 1 dinh), mau doi qua _Color
Shader "_Game/VFX/Glow Wall" {
    Properties {
        [HDR] _Color ("Color", Color) = (0.2, 1.6, 0.9, 1)
        _Alpha ("Alpha", Range(0, 2)) = 1

        [Header(Gradient)]
        _Intensity ("Intensity", Range(0, 4)) = 1.2
        _Falloff ("Falloff (Cang lon cang mo nhanh)", Range(0.2, 8)) = 2.2
        _BaseLine ("Base Line", Range(0, 4)) = 1.2
        _BaseLineWidth ("Base Line Width", Range(0.01, 0.5)) = 0.08
        _Whiten ("Whiten", Range(0, 1)) = 0.3

        [Header(Streaks)]
        _StreakCount ("Streak Count", Float) = 24
        _StreakAmount ("Streak Amount", Range(0, 1)) = 0.25
        _StreakSpeed ("Streak Speed", Float) = 0.4

        [Header(Animation)]
        _PulseSpeed ("Pulse Speed", Float) = 2.5
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2

        [Header(Blend)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend (One = Additive, OneMinusSrcAlpha = Alpha)", Float) = 10
    }

    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass {
            Name "GlowWall"
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
                half _Intensity;
                half _Falloff;
                half _BaseLine;
                half _BaseLineWidth;
                half _Whiten;
                float _StreakCount;
                half _StreakAmount;
                float _StreakSpeed;
                float _PulseSpeed;
                half _PulseAmount;
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
                float height01 = saturate(input.uv.y);

                // gradient chinh: dam o chan, mo dan len dinh, ve 0 o dinh
                float gradient = pow(1.0 - height01, _Falloff) * _Intensity;
                float baseLine = (1.0 - smoothstep(0.0, _BaseLineWidth, height01)) * _BaseLine;

                // vet sang doc chay len nhe cho tuong bot phang
                float bandCount = max(1.0, round(_StreakCount));
                float streak = 0.5 + 0.5 * sin((input.uv.x * bandCount + sin(input.uv.x * 6.2831853 * 3.0 + time) * 0.5) * 6.2831853);
                streak = lerp(1.0, 0.6 + streak * 0.8, _StreakAmount);
                streak *= lerp(1.0, 0.75 + 0.25 * sin((height01 - time * _StreakSpeed) * 12.0), _StreakAmount);

                float pulse = 1.0 + sin(time * _PulseSpeed) * _PulseAmount;
                float mask = (gradient * streak + baseLine) * pulse;

                float3 baseColor = _Color.rgb;
                float brightness = max(max(baseColor.r, baseColor.g), baseColor.b);
                float3 color = lerp(baseColor, float3(1.0, 1.0, 1.0) * brightness, _Whiten * saturate(baseLine + gradient * 0.5));

                float alpha = saturate(mask) * _Alpha * _Color.a;

                // output premultiplied: dung duoc ca alpha blend (One OneMinusSrcAlpha) lan additive (One One)
                return half4(color * mask * _Alpha * _Color.a, alpha);
            }
            ENDHLSL
        }
    }
}
