Shader "Custom/SpriteRarityOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0.2, 0.55, 1, 1)
        _OutlineThickness ("Outline Thickness", Range(0, 4)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType"="Unlit"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SpriteOutlineUnlit"
            Tags { "LightMode"="Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _OutlineColor;
                float _OutlineThickness;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half SampleAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                const int maxRadius = 4;
                float radius = saturate(_OutlineThickness / maxRadius) * maxRadius;
                int radiusSteps = clamp((int)ceil(radius), 0, maxRadius);

                half outlineAlpha = 0;

                [unroll]
                for (int x = -maxRadius; x <= maxRadius; x++)
                {
                    [unroll]
                    for (int y = -maxRadius; y <= maxRadius; y++)
                    {
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        float2 offsetSteps = float2(x, y);
                        float distanceFromCenter = length(offsetSteps);

                        if (distanceFromCenter > radius || abs(x) > radiusSteps || abs(y) > radiusSteps)
                        {
                            continue;
                        }

                        float2 sampleOffset = _MainTex_TexelSize.xy * offsetSteps;
                        outlineAlpha = max(outlineAlpha, SampleAlpha(input.uv + sampleOffset));
                    }
                }

                outlineAlpha = saturate(outlineAlpha - sprite.a);

                half4 outline = _OutlineColor;
                outline.a *= outlineAlpha;

                half3 rgb = lerp(outline.rgb, sprite.rgb, sprite.a);
                half alpha = max(sprite.a, outline.a);

                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
