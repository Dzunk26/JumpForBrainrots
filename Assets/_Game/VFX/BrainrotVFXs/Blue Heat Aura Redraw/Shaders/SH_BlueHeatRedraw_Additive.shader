// shader cho VFX chuyen tu Roblox: texture x vertex color x mau HDR, tron premultiplied
// _LightEmission giong Roblox: 1 = cong sang (additive), 0 = alpha thuong, o giua = pha tron
// _AlphaGamma: Roblox tron mau trong gamma space, project chay Linear -> nang alpha len luy thua de do dam giong Roblox
// Beam: 1 hat Vertical Billboard, truc doc cua texture chay doc beam (Attachment0 -> 1), truc ngang theo be rong
//   _Repeat / _ScrollSpeed = TextureLength / TextureSpeed, _Width0 / _Width1 = be rong 2 dau (ti le so voi be rong hat)
//   _RampTex = Color + Transparency doc theo chieu dai beam
Shader "_Game/VFX/Blue Heat Aura Redraw Additive" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _AlphaGamma ("Alpha Gamma", Range(0.5, 3)) = 1
        _LightEmission ("Light Emission", Range(0, 1)) = 1

        [Header(Beam)]
        [ToggleUI] _IsBeam ("Is Beam", Float) = 0
        [NoScaleOffset] _RampTex ("Ramp (Color + Alpha doc beam)", 2D) = "white" {}
        _Repeat ("Repeat (TextureLength)", Float) = 1
        _ScrollSpeed ("Scroll Speed (TextureSpeed)", Float) = 0
        _Width0 ("Width 0", Range(0, 1)) = 1
        _Width1 ("Width 1", Range(0, 1)) = 1
    }

    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "PreviewType" = "Plane" }

        Pass {
            Name "RobloxAdditive"
            Tags { "LightMode" = "UniversalForward" }

            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_RampTex);
            SAMPLER(sampler_RampTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _AlphaGamma;
                half _LightEmission;
                float _IsBeam;
                float _Repeat;
                float _ScrollSpeed;
                float _Width0;
                float _Width1;
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
                float2 uv = TRANSFORM_TEX(input.uv, _MainTex);
                half beamMask = 1;
                half4 ramp = half4(1, 1, 1, 1);

                if (_IsBeam > 0.5) {
                    // thu hep be rong tu Width0 (day) den Width1 (dinh)
                    float width = max(lerp(_Width0, _Width1, input.uv.y), 0.001);
                    float u = (input.uv.x - 0.5) / width + 0.5;
                    beamMask = step(0.0, u) * step(u, 1.0);
                    uv = float2(u, input.uv.y * _Repeat - _Time.y * _ScrollSpeed);
                    ramp = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(input.uv.y, 0.5));
                }

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half alpha = pow(saturate(tex.a * input.color.a * ramp.a * beamMask), _AlphaGamma) * _Color.a;
                half3 color = tex.rgb * input.color.rgb * ramp.rgb * _Color.rgb;

                return half4(color * alpha, alpha * (1 - _LightEmission));
            }
            ENDHLSL
        }
    }
}
