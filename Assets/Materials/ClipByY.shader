Shader "Custom/ClipByY"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _XThreshold ("X Threshold", Float) = 0.0
        _YThreshold ("Y Threshold", Float) = 0.0
        _Tangent ("Tangent", Float) = 0.0
        _InverseBlendRate ("BlendRate", Range(0.0, 1.0)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha 
        LOD 100

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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _XThreshold;
            float _YThreshold;
            float _Tangent;
            fixed _InverseBlendRate;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float y_line = tan(_Tangent) * (i.worldPos.x - _XThreshold) + _YThreshold;
    
                if (i.worldPos.y > y_line) // Above
                {

                }
                else if(i.worldPos.y < y_line) // Below
                {
                    discard;
                }
                else // On the line
                {
                    discard;
                }

                /*// Y座標がしきい値より手前の場合は描画しない
                if (i.worldPos.y < _YThreshold)
                {
                    discard;
                }*/
                
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a = _Color.a;
                col.r = abs(col.r - _InverseBlendRate);
                col.g = abs(col.g - _InverseBlendRate);
                col.b = abs(col.b - _InverseBlendRate);
                return col;
            }
            ENDCG
        }
    }
}