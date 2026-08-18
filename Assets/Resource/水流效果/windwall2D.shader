Shader "Custom/WindWall2D"
{
    Properties
    {
        _MainTex("Wind Texture", 2D) = "white" {}
        _NoiseTex("Noise",2D) = "gray"{}
        _Color("Tint", Color) = (0.7,0.85,0.95,0.6)
        _FlowSpeed("Flow Speed",Float) = 0.6
        _Distort("Distort",Float) = 0.02
        _EdgeBright("Edge Bright",Float) = 1.0
        _FadeTop("上边渐变强度",Range(0,1)) = 0.2
        _FadeBottom("下边渐变强度",Range(0,1)) = 0.2
    }
        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "IgnoreProjector" = "True"
            }
            LOD 100
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };
                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float2 uvNoise : TEXCOORD1;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                sampler2D _NoiseTex;
                float4 _NoiseTex_ST;
                float4 _Color;
                float _FlowSpeed;
                float _Distort;
                float _EdgeBright;
                float _FadeTop;
                float _FadeBottom;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                    o.uvNoise = TRANSFORM_TEX(v.uv,_NoiseTex);
                    return o;
                }

                fixed4 frag(v2f i) :SV_Target
                {
                    float2 uvWind = i.uv;
                    float2 uvN = i.uvNoise;
                    uvN.y += _Time.y * _FlowSpeed * 0.4;
                    float noise = tex2D(_NoiseTex,uvN).r;

                    uvWind.x += (noise - 0.5) * _Distort;
                    uvWind.y += _Time.y * _FlowSpeed;

                    fixed4 col = tex2D(_MainTex,uvWind) * _Color;

                    //左右边缘提亮
                    float edgeX = abs(i.uv.x - 0.5) * 2;
                    col.rgb *= lerp(1,2.2,saturate(edgeX * _EdgeBright));

                    //上下窄边渐变，只边缘一点点淡出
                    float fadeRange = 0.08;
                    float topFactor = smoothstep(1 - fadeRange,1,i.uv.y);
                    float botFactor = smoothstep(0,fadeRange,i.uv.y);

                    float topAlpha = 1 - topFactor * _FadeTop;
                    float botAlpha = botFactor + (1 - botFactor) * (1 - _FadeBottom);

                    float alphaMask = saturate(topAlpha * botAlpha);
                    col.a *= alphaMask;

                    return col;
                }
                ENDCG
            }
        }
            FallBack "Unlit/Transparent"
}