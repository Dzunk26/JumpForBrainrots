// shader nen the rarity cho ui (index): gradient 4 mau chay vong + luoi o vuong noi cuon cheo
// + rainbow (infinity) + sao lap lanh + vet sang quet + bo goc va vien
// dung voi Image khong co Source Image (uv 0..1), ty le khung tu tinh nen o luon vuong
Shader "_Game/UI/Rarity Background" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Gradient)]
        _Color0 ("Color 0", Color) = (0.235, 0.333, 0.012, 1)
        _Color1 ("Color 1", Color) = (0.384, 0.584, 0.039, 1)
        _Color2 ("Color 2", Color) = (0.541, 0.839, 0.039, 1)
        _Color3 ("Color 3", Color) = (0.416, 0.659, 0.024, 1)
        _GradientAngle ("Gradient Angle", Range(-180, 180)) = 0
        _GradientScale ("Gradient Scale", Float) = 0.5
        _GradientSpeed ("Gradient Speed", Float) = 0.12
        _GradientWave ("Gradient Wave", Range(0, 0.3)) = 0.04
        _GradientWaveFrequency ("Gradient Wave Frequency", Float) = 2.5

        [Header(Rainbow)]
        _Rainbow ("Rainbow Blend", Range(0, 1)) = 0
        _RainbowSaturation ("Rainbow Saturation", Range(0, 1)) = 0.35
        _RainbowValue ("Rainbow Value", Range(0, 1)) = 0.9

        [Header(Tiles)]
        _TileCount ("Tile Count (Per Height)", Float) = 8.5
        _TileSize ("Tile Size", Range(0.1, 1)) = 0.52
        _TileRoundness ("Tile Roundness", Range(0, 1)) = 0.35
        _TileRotation ("Tile Rotation", Range(0, 90)) = 45
        _TileBrightness ("Tile Brightness", Range(-0.5, 0.5)) = 0.06
        _TileShadow ("Tile Shadow", Range(0, 1)) = 0.22
        _TileHighlight ("Tile Highlight", Range(0, 1)) = 0.1
        _TileEdge ("Tile Edge Width", Range(0, 0.2)) = 0.07
        _PatternSpeed ("Pattern Speed (XY)", Vector) = (0.25, 0.25, 0, 0)

        [Header(Sparkle)]
        _SparkleStrength ("Sparkle Strength", Range(0, 2)) = 0
        _SparkleColor ("Sparkle Color", Color) = (1, 1, 1, 1)
        _SparkleDensity ("Sparkle Density", Float) = 4
        _SparkleSize ("Sparkle Size", Range(0.1, 1)) = 0.45
        _SparkleSpeed ("Sparkle Speed", Float) = 2
        _SparkleChance ("Sparkle Chance", Range(0, 1)) = 0.45

        [Header(Shine)]
        _ShineStrength ("Shine Strength", Range(0, 1)) = 0
        _ShineWidth ("Shine Width", Range(0.01, 0.5)) = 0.12
        _ShineInterval ("Shine Interval", Float) = 3.5
        _ShineSpeed ("Shine Speed", Float) = 1.2

        [Header(Frame)]
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.06
        _BorderWidth ("Border Width", Range(0, 0.1)) = 0.018
        _BorderColor ("Border Color", Color) = (0.07, 0.07, 0.07, 1)
        _EdgeShade ("Edge Shade", Range(0, 1)) = 0.22
        _EdgeShadeSize ("Edge Shade Size", Range(0.01, 0.5)) = 0.15

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader {
        Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            float4 _Color0;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float _GradientAngle;
            float _GradientScale;
            float _GradientSpeed;
            float _GradientWave;
            float _GradientWaveFrequency;

            float _Rainbow;
            float _RainbowSaturation;
            float _RainbowValue;

            float _TileCount;
            float _TileSize;
            float _TileRoundness;
            float _TileRotation;
            float _TileBrightness;
            float _TileShadow;
            float _TileHighlight;
            float _TileEdge;
            float4 _PatternSpeed;

            float _SparkleStrength;
            float4 _SparkleColor;
            float _SparkleDensity;
            float _SparkleSize;
            float _SparkleSpeed;
            float _SparkleChance;

            float _ShineStrength;
            float _ShineWidth;
            float _ShineInterval;
            float _ShineSpeed;

            float _CornerRadius;
            float _BorderWidth;
            float4 _BorderColor;
            float _EdgeShade;
            float _EdgeShadeSize;

            // tinh toan mau trong gamma space de giong voi mau chon tren inspector
            float3 ToGamma(float3 color) {
                #ifdef UNITY_COLORSPACE_GAMMA
                return color;
                #else
                return LinearToGammaSpace(color);
                #endif
            }

            float3 ToLinear(float3 color) {
                #ifdef UNITY_COLORSPACE_GAMMA
                return color;
                #else
                return GammaToLinearSpace(color);
                #endif
            }

            float SdRoundBox(float2 p, float2 halfSize, float radius) {
                float2 d = abs(p) - halfSize + radius;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - radius;
            }

            // xoay toa do trong o, 45 do = hinh thoi
            float2 RotateCell(float2 cell, float angle) {
                float s = sin(angle);
                float c = cos(angle);
                return float2(c * cell.x + s * cell.y, -s * cell.x + c * cell.y);
            }

            float3 HsvToRgb(float hue, float saturation, float value) {
                float3 k = saturate(abs(frac(hue + float3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0);
                return value * lerp(float3(1.0, 1.0, 1.0), k, saturation);
            }

            float2 Hash22(float2 p) {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            // gradient vong 4 mau: 0 -> 1 -> 2 -> 3 -> 0
            float3 GetGradientColor(float t) {
                float s = frac(t) * 4.0;
                float index = floor(s);
                float blend = smoothstep(0.0, 1.0, s - index);
                float3 c0 = ToGamma(_Color0.rgb);
                float3 c1 = ToGamma(_Color1.rgb);
                float3 c2 = ToGamma(_Color2.rgb);
                float3 c3 = ToGamma(_Color3.rgb);
                float3 fromColor = index < 0.5 ? c0 : (index < 1.5 ? c1 : (index < 2.5 ? c2 : c3));
                float3 toColor = index < 0.5 ? c1 : (index < 1.5 ? c2 : (index < 2.5 ? c3 : c0));
                return lerp(fromColor, toColor, blend);
            }

            v2f vert(appdata_t v) {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target {
                float2 uv = IN.texcoord;
                float time = _Time.y;

                // ty le rong / cao cua khung, lay tu dao ham uv nen khong can set tay
                float aspect = length(ddy(uv)) / max(length(ddx(uv)), 1e-6);
                float2 p = (uv - 0.5) * float2(aspect, 1.0);

                // gradient chay theo huong goc, co luon song nhe
                float angle = radians(_GradientAngle);
                float2 direction = float2(sin(angle), -cos(angle));
                float along = dot(p, direction);
                float across = dot(p, float2(direction.y, -direction.x));
                float g = along * _GradientScale + time * _GradientSpeed + sin(across * _GradientWaveFrequency * 6.2832 + time * 1.3) * _GradientWave;
                float3 color = lerp(GetGradientColor(g), HsvToRgb(frac(g), _RainbowSaturation, _RainbowValue), _Rainbow);

                // luoi o noi (xoay 45 do = hinh thoi): canh duoi toi, canh tren sang
                float2 tileUV = p * _TileCount + time * _PatternSpeed.xy;
                float tileAA = fwidth(tileUV.y) * 0.75;
                float2 cell = RotateCell(frac(tileUV) - 0.5, radians(_TileRotation));
                float halfSize = _TileSize * 0.5;
                float radius = _TileRoundness * halfSize;
                float face = 1.0 - smoothstep(-tileAA, tileAA, SdRoundBox(cell, halfSize, radius));
                float shadow = face * smoothstep(-tileAA, tileAA, SdRoundBox(cell - _TileEdge, halfSize, radius));
                float highlight = face * smoothstep(-tileAA, tileAA, SdRoundBox(cell + _TileEdge, halfSize, radius)) * (1.0 - shadow);
                color *= 1.0 + _TileBrightness * face;
                color *= 1.0 - _TileShadow * shadow;
                color = lerp(color, float3(1.0, 1.0, 1.0), _TileHighlight * highlight);

                // sao lap lanh ngau nhien theo o
                if (_SparkleStrength > 0.0) {
                    float2 sparkleUV = p * _SparkleDensity + float2(0.0, time * 0.05);
                    float2 hash = Hash22(floor(sparkleUV));
                    float2 q = (frac(sparkleUV) - 0.5 - (hash - 0.5) * 0.5) / _SparkleSize;
                    float twinkle = max(0.0, sin(time * _SparkleSpeed + hash.x * 6.2832));
                    twinkle = twinkle * twinkle * twinkle;
                    float core = smoothstep(0.14, 0.0, length(q));
                    float rays = smoothstep(0.07, 0.0, abs(q.x)) * smoothstep(0.5, 0.0, abs(q.y)) + smoothstep(0.07, 0.0, abs(q.y)) * smoothstep(0.5, 0.0, abs(q.x));
                    color += ToGamma(_SparkleColor.rgb) * (core + rays * 0.7) * twinkle * _SparkleStrength * step(hash.y, _SparkleChance);
                }

                // vet sang quet cheo theo chu ky
                if (_ShineStrength > 0.0) {
                    float shineAxis = dot(p, normalize(float2(1.0, 0.55)));
                    float shineRange = (aspect + 1.0) * 0.6 + _ShineWidth;
                    float shinePos = frac(time / _ShineInterval) * _ShineInterval * _ShineSpeed - shineRange;
                    float shine = smoothstep(_ShineWidth, 0.0, abs(shineAxis - shinePos));
                    color = lerp(color, float3(1.0, 1.0, 1.0), shine * _ShineStrength);
                }

                // bo goc, toi dan sat vien, vien ngoai
                float frameDist = SdRoundBox(p, float2(aspect * 0.5, 0.5), _CornerRadius);
                float frameAA = fwidth(p.y);
                float inside = 1.0 - smoothstep(-frameAA, frameAA, frameDist);
                color *= 1.0 - _EdgeShade * smoothstep(-_BorderWidth - _EdgeShadeSize, -_BorderWidth, frameDist);
                float border = smoothstep(-_BorderWidth - frameAA, -_BorderWidth + frameAA, frameDist);
                color = lerp(color, ToGamma(_BorderColor.rgb), border * _BorderColor.a);

                half4 tex = tex2D(_MainTex, TRANSFORM_TEX(uv, _MainTex)) + _TextureSampleAdd;
                half4 finalColor = half4(ToLinear(saturate(color)), inside) * tex * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}
