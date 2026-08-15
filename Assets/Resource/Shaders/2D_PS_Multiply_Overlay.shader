Shader "Custom/2D_PS_Multiply_Overlay"
{
    Properties
    {
        _MainTex ("图层贴图", 2D) = "white" {}
        _Opacity ("图层不透明度", Range(0,1)) = 1.0
        _BlendMode ("混合模式 0=正片叠底 1=强光", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }

        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            struct Varyings
            {
                float4 posHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 vertColor : COLOR;
                float2 screenUV : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float _Opacity;
            float _BlendMode;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.posHCS = TransformObjectToHClip(input.posOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.vertColor = input.vertexColor;
                // 屏幕UV，用于采样底层画面
                output.screenUV = output.posHCS.xy / _ScreenParams.xy;
                return output;
            }

            // PS 正片叠底 Multiply
            float3 MultiplyBlend(float3 baseCol, float3 topCol)
            {
                return baseCol * topCol;
            }

            // PS 强光 Overlay
            float3 OverlayBlend(float3 baseCol, float3 topCol)
            {
                float3 res;
                res.r = topCol.r < 0.5 ? 2 * baseCol.r * topCol.r : 1 - 2 * (1 - baseCol.r) * (1 - topCol.r);
                res.g = topCol.g < 0.5 ? 2 * baseCol.g * topCol.g : 1 - 2 * (1 - baseCol.g) * (1 - topCol.g);
                res.b = topCol.b < 0.5 ? 2 * baseCol.b * topCol.b : 1 - 2 * (1 - baseCol.b) * (1 - topCol.b);
                return res;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 上层贴图颜色
                half4 top = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.vertColor;
                top.a *= _Opacity;
                // 底层场景颜色
                half4 baseTex = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, input.screenUV);
                half3 resultRGB;

                // 切换混合模式
                if (_BlendMode < 0.5)
                {
                    resultRGB = MultiplyBlend(baseTex.rgb, top.rgb);
                }
                else
                {
                    resultRGB = OverlayBlend(baseTex.rgb, top.rgb);
                }

                // 透明度插值混合
                half3 finalCol = lerp(baseTex.rgb, resultRGB, top.a);
                return half4(finalCol, baseTex.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/2D/Sprite-Unlit-Default"
}