Shader "Custom/NetHatch"
{
    Properties
    {
        _MainTex ("Hatch Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Width ("World Width", Float) = 1
        _Length ("World Length", Float) = 1
        _TileWorldSize ("Tile World Size", Float) = 1
        _HoleRadius ("Reveal Hole Radius", Range(0, 1.5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _Color;
            float _Width;
            float _Length;
            float _TileWorldSize;
            float _HoleRadius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 centeredUv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.centeredUv = v.uv - 0.5;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float x = i.centeredUv.x;
                float y = i.centeredUv.y;

                float topToBottom = saturate(0.5 - y);
                float allowedHalfWidth = lerp(0.08, 0.5, topToBottom);

                float distFromCenter = length(float2(x * 2.0, y * 2.0));
                float revealed = distFromCenter >= _HoleRadius;

                float2 worldLikeUv;
                worldLikeUv.x = (x * _Width) / _TileWorldSize;
                worldLikeUv.y = (y * _Length) / _TileWorldSize;

                fixed4 col = tex2D(_MainTex, worldLikeUv) * _Color;
                col.a *= revealed;

                return col;
            }
            ENDCG
        }
    }
}