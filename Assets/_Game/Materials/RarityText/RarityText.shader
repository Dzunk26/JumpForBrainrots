// shader chu rarity (textmeshpro sdf): mau chu chay giong het RarityBackground
// gradient 4 mau chay vong + rainbow + sao lap lanh + vet sang quet, giu outline / underlay cua tmp
// toa do lay tu face uv cua tmp -> text nen de Horizontal Mapping = Paragraph, Vertical Mapping = Paragraph
// _PatternScale = chieu cao chu / chieu cao the background, de gradient / sparkle tren chu cung kich thuoc voi tren the
Shader "_Game/Text/Rarity Text" {
    Properties {
        [Header(Rarity Gradient)]
        _Color0 ("Color 0", Color) = (0.235, 0.333, 0.012, 1)
        _Color1 ("Color 1", Color) = (0.384, 0.584, 0.039, 1)
        _Color2 ("Color 2", Color) = (0.541, 0.839, 0.039, 1)
        _Color3 ("Color 3", Color) = (0.416, 0.659, 0.024, 1)
        _GradientAngle ("Gradient Angle", Range(-180, 180)) = 0
        _ColorGradientScale ("Gradient Scale", Float) = 0.5
        _GradientSpeed ("Gradient Speed", Float) = 0.12
        _GradientWave ("Gradient Wave", Range(0, 0.3)) = 0.04
        _GradientWaveFrequency ("Gradient Wave Frequency", Float) = 2.5
        _PatternScale ("Pattern Scale (Text Height / Card Height)", Float) = 0.3
        _FaceBrightness ("Face Brightness", Range(0.5, 2)) = 1

        [Header(Rainbow)]
        _Rainbow ("Rainbow Blend", Range(0, 1)) = 0
        _RainbowSaturation ("Rainbow Saturation", Range(0, 1)) = 0.35
        _RainbowValue ("Rainbow Value", Range(0, 1)) = 0.9

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

        [Header(TextMeshPro)]
        [HDR] _FaceColor ("Face Tint", Color) = (1, 1, 1, 1)
        _FaceDilate ("Face Dilate", Range(-1, 1)) = 0

        [HDR] _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Thickness", Range(0, 1)) = 0
        _OutlineSoftness ("Outline Softness", Range(0, 1)) = 0

        [Toggle(UNDERLAY_ON)] _UseUnderlay ("Use Underlay", Float) = 0
        [HDR] _UnderlayColor ("Underlay Color", Color) = (0, 0, 0, 0.5)
        _UnderlayOffsetX ("Underlay OffsetX", Range(-1, 1)) = 0
        _UnderlayOffsetY ("Underlay OffsetY", Range(-1, 1)) = 0
        _UnderlayDilate ("Underlay Dilate", Range(-1, 1)) = 0
        _UnderlaySoftness ("Underlay Softness", Range(0, 1)) = 0

        _WeightNormal ("Weight Normal", Float) = 0
        _WeightBold ("Weight Bold", Float) = 0.5

        [HideInInspector] _ShaderFlags ("Flags", Float) = 0
        [HideInInspector] _ScaleRatioA ("Scale RatioA", Float) = 1
        [HideInInspector] _ScaleRatioB ("Scale RatioB", Float) = 1
        [HideInInspector] _ScaleRatioC ("Scale RatioC", Float) = 1

        _MainTex ("Font Atlas", 2D) = "white" {}
        _TextureWidth ("Texture Width", Float) = 512
        _TextureHeight ("Texture Height", Float) = 512
        _GradientScale ("SDF Gradient Scale", Float) = 5
        _ScaleX ("Scale X", Float) = 1
        _ScaleY ("Scale Y", Float) = 1
        _PerspectiveFilter ("Perspective Correction", Range(0, 1)) = 0.875
        _Sharpness ("Sharpness", Range(-1, 1)) = 0

        [HideInInspector] _VertexOffsetX ("Vertex OffsetX", Float) = 0
        [HideInInspector] _VertexOffsetY ("Vertex OffsetY", Float) = 0

        [HideInInspector] _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
        [HideInInspector] _MaskSoftnessX ("Mask SoftnessX", Float) = 0
        [HideInInspector] _MaskSoftnessY ("Mask SoftnessY", Float) = 0

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector] _CullMode ("Cull Mode", Float) = 0
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader {
        Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull [_CullMode]
        ZWrite Off
        Lighting Off
        Fog { Mode Off }
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma shader_feature_local __ UNDERLAY_ON

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                fixed4 color : COLOR;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };

            struct v2f {
                UNITY_VERTEX_OUTPUT_STEREO
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 texcoord0 : TEXCOORD0;     // atlas uv (xy), face uv (zw)
                half4 param : TEXCOORD1;          // scale, bias - outline, bias + outline, bias
                half4 mask : TEXCOORD2;           // vi tri clip (xy), softness (zw)
                half outlineBlend : TEXCOORD3;
                #if UNDERLAY_ON
                float2 underlayUV : TEXCOORD4;
                half2 underlayParam : TEXCOORD5;  // scale, bias
                #endif
            };

            sampler2D _MainTex;
            float _TextureWidth;
            float _TextureHeight;
            float _GradientScale;
            float _ScaleX;
            float _ScaleY;
            float _PerspectiveFilter;
            float _Sharpness;
            float _VertexOffsetX;
            float _VertexOffsetY;
            float _ScaleRatioA;
            float _ScaleRatioC;
            float _WeightNormal;
            float _WeightBold;
            float _FaceDilate;
            float4 _FaceColor;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineSoftness;
            float4 _UnderlayColor;
            float _UnderlayOffsetX;
            float _UnderlayOffsetY;
            float _UnderlayDilate;
            float _UnderlaySoftness;
            float4 _ClipRect;
            float _MaskSoftnessX;
            float _MaskSoftnessY;

            float4 _Color0;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float _GradientAngle;
            float _ColorGradientScale;
            float _GradientSpeed;
            float _GradientWave;
            float _GradientWaveFrequency;
            float _PatternScale;
            float _FaceBrightness;

            float _Rainbow;
            float _RainbowSaturation;
            float _RainbowValue;

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

            // tmp dong goi face uv vao 1 float
            float2 UnpackUV(float uv) {
                float2 output;
                output.x = floor(uv / 4096);
                output.y = uv - 4096 * output.x;
                return output * 0.001953125;
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

            // mau chu theo face uv, cung cong thuc voi RarityBackground (bo phan khung va luoi o)
            float3 GetRarityColor(float2 uv) {
                float time = _Time.y;

                float aspect = length(ddy(uv)) / max(length(ddx(uv)), 1e-6);
                float2 p = (uv - 0.5) * float2(aspect, 1.0) * _PatternScale;

                float angle = radians(_GradientAngle);
                float2 direction = float2(sin(angle), -cos(angle));
                float along = dot(p, direction);
                float across = dot(p, float2(direction.y, -direction.x));
                float g = along * _ColorGradientScale + time * _GradientSpeed + sin(across * _GradientWaveFrequency * 6.2832 + time * 1.3) * _GradientWave;
                float3 color = lerp(GetGradientColor(g), HsvToRgb(frac(g), _RainbowSaturation, _RainbowValue), _Rainbow);

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

                if (_ShineStrength > 0.0) {
                    float shineAxis = dot(p, normalize(float2(1.0, 0.55)));
                    float shineRange = (aspect + 1.0) * 0.6 * _PatternScale + _ShineWidth;
                    float shinePos = frac(time / _ShineInterval) * _ShineInterval * _ShineSpeed - shineRange;
                    float shine = smoothstep(_ShineWidth, 0.0, abs(shineAxis - shinePos));
                    color = lerp(color, float3(1.0, 1.0, 1.0), shine * _ShineStrength);
                }

                return ToLinear(saturate(color * _FaceBrightness));
            }

            v2f vert(appdata_t v) {
                v2f OUT;
                UNITY_INITIALIZE_OUTPUT(v2f, OUT);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float bold = step(v.texcoord1.y, 0);

                float4 vert = v.vertex;
                vert.x += _VertexOffsetX;
                vert.y += _VertexOffsetY;
                float4 vPosition = UnityObjectToClipPos(vert);

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float scale = rsqrt(dot(pixelSize, pixelSize));
                scale *= abs(v.texcoord1.y) * _GradientScale * (_Sharpness + 1);
                if (UNITY_MATRIX_P[3][3] == 0) scale = lerp(abs(scale) * (1 - _PerspectiveFilter), scale, abs(dot(UnityObjectToWorldNormal(v.normal.xyz), normalize(WorldSpaceViewDir(vert)))));

                float weight = lerp(_WeightNormal, _WeightBold, bold) / 4.0;
                weight = (weight + _FaceDilate) * _ScaleRatioA * 0.5;

                float layerScale = scale;

                scale /= 1 + (_OutlineSoftness * _ScaleRatioA * scale);
                float bias = (0.5 - weight) * scale - 0.5;
                float outline = _OutlineWidth * _ScaleRatioA * 0.5 * scale;

                #if UNDERLAY_ON
                layerScale /= 1 + ((_UnderlaySoftness * _ScaleRatioC) * layerScale);
                float layerBias = (0.5 - weight) * layerScale - 0.5 - ((_UnderlayDilate * _ScaleRatioC) * 0.5 * layerScale);
                float2 layerOffset = float2(-(_UnderlayOffsetX * _ScaleRatioC) * _GradientScale / _TextureWidth, -(_UnderlayOffsetY * _ScaleRatioC) * _GradientScale / _TextureHeight);
                OUT.underlayUV = v.texcoord0 + layerOffset;
                OUT.underlayParam = half2(layerScale, layerBias);
                #endif

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);

                OUT.vertex = vPosition;
                OUT.color = v.color;
                OUT.texcoord0 = float4(v.texcoord0, UnpackUV(v.texcoord1.x));
                OUT.param = half4(scale, bias - outline, bias + outline, bias);
                OUT.mask = half4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_MaskSoftnessX, _MaskSoftnessY) + pixelSize.xy));
                OUT.outlineBlend = sqrt(min(1.0, outline * 2));
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target {
                // mau chu (premultiplied alpha giong tmp)
                half4 faceColor = half4(GetRarityColor(IN.texcoord0.zw), 1) * IN.color * _FaceColor;
                faceColor.rgb *= faceColor.a;

                half4 outlineColor = _OutlineColor;
                outlineColor.a *= IN.color.a;
                outlineColor.rgb *= outlineColor.a;
                outlineColor = lerp(faceColor, outlineColor, IN.outlineBlend);

                half d = tex2D(_MainTex, IN.texcoord0.xy).a * IN.param.x;
                half4 c = lerp(outlineColor, faceColor, saturate(d - IN.param.z));
                c *= saturate(d - IN.param.y);

                #if UNDERLAY_ON
                d = tex2D(_MainTex, IN.underlayUV).a * IN.underlayParam.x;
                c += float4(_UnderlayColor.rgb * _UnderlayColor.a, _UnderlayColor.a) * IN.color.a * saturate(d - IN.underlayParam.y) * (1 - c.a);
                #endif

                #if UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                c *= m.x * m.y;
                #endif

                #if UNITY_UI_ALPHACLIP
                clip(c.a - 0.001);
                #endif

                return c;
            }
            ENDCG
        }
    }
}
