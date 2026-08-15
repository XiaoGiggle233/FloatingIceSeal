Shader "Custom/2D_PS_Multiply_Overlay"
{
    Properties
    {
        [MainTexture] _MainTex ("图层贴图", 2D) = "white" {}
        _Opacity ("图层不透明度", Range(0,1)) = 1.0
        [KeywordEnum(Multiply, Overlay)] _BlendMode ("混合模式", Float) = 1
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

        // 正片叠底（PS Multiply）：结果 = 底层 * 图层。
        // 本项目的 URP 2D Renderer 不生成 _CameraOpaqueTexture（无法采样底层），
        // 改用固定功能混合 Blend DstColor Zero（dst * src），数学上等价
        Pass
        {
            Name "Multiply"
            Blend DstColor Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ _BLENDMODE_OVERLAY
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 vertColor : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Opacity;
            CBUFFER_END

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.vertColor = input.vertexColor;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
#ifdef _BLENDMODE_OVERLAY
                clip(-1);
                return half4(0, 0, 0, 0);
#else
                half4 top = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.vertColor;
                half a = top.a * _Opacity;
                // 透明度插值：a=0 时乘 1（不改变底层）
                half3 src = lerp(half3(1, 1, 1), top.rgb, a);
                return half4(src, 1);
#endif
            }
            ENDHLSL
        }

        // 强光（Overlay）：图层颜色 >= 0.5 时 Overlay 公式与滤色 Screen 一致，
        // 光源贴图为白/灰色，Screen 近似结果相同，用 Blend OneMinusDstColor One
        Pass
        {
            Name "Overlay"
            Blend OneMinusDstColor One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ _BLENDMODE_OVERLAY
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 vertColor : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Opacity;
            CBUFFER_END

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.vertColor = input.vertexColor;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
#ifndef _BLENDMODE_OVERLAY
                clip(-1);
                return half4(0, 0, 0, 0);
#else
                half4 top = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.vertColor;
                half a = top.a * _Opacity;
                // 颜色预乘透明度，等价于 lerp(底层, Screen(底层, top), a)
                half3 src = top.rgb * a;
                return half4(src, 1);
#endif
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
