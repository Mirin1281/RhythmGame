Shader "Custom/InverseColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlendRate ("BlendRate", Range(0.0, 1.0)) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                fixed2 uv : TEXCOORD0;
                fixed4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                fixed2 uv : TEXCOORD0;
                fixed4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed _BlendRate;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= i.color;
                col.r = abs(col.r - _BlendRate);
                col.g = abs(col.g - _BlendRate);
                col.b = abs(col.b - _BlendRate);
                return col;
            }
            ENDCG
        }
    }
}