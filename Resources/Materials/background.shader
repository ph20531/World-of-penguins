Shader "Custom/BackgroundSpriteShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color1 ("Color 1", Color) = (1,1,1,1)
        _Color2 ("Color 2", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
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

            half4 _Color1;
            half4 _Color2;
            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 텍스처에서 색상을 샘플링하여 기본 색상을 가져옵니다.
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // 그라디언트 계산
                float gradient = i.uv.y;

                // 그라디언트를 기준으로 컬러 블렌딩
                fixed4 finalColor = lerp(_Color1 * texColor, _Color2 * texColor, gradient);

                return finalColor;
            }
            ENDCG
        }
    }
}
