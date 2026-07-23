Shader "Custom/MultiplyShader" // 1. 定义Shader在材质面板中的路径
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // 2. 声明一个贴图属性，用于接收你的2D图片
    }
    SubShader
    {
        // 3. 设置标签，指明这是一个透明队列的对象，并关闭投影器影响
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        
        // 4. ★★★ 核心步骤：设置混合模式为正片叠底 (Multiply) ★★★
        //    此命令告诉GPU将当前像素颜色（源）与帧缓冲区中已有的颜色（目标）相乘[reference:4][reference:5]
        Blend DstColor Zero
        
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 采样主纹理
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}