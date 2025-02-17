Shader "Custom/Negative" {
    Properties {
        _BlendRate ("BlendRate", Range(0, 1)) = 1
    }

    SubShader {
        Tags { "Queue" = "Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local UNITY_UI_CLIP_RECT

            struct appdata {
                fixed2 uv : TEXCOORD0;
                fixed4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f {
                fixed2 uv : TEXCOORD0;
                fixed4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 mask : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed _BlendRate;
            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;

#ifdef UNITY_UI_CLIP_RECT
                float2 pixelSize = o.vertex.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskXY = v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw;
                float2 maskZW = 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy));
                o.mask = float4(maskXY, maskZW);
#else
                o.mask = 0;
#endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                col.r = abs(col.r - _BlendRate);
                col.g = abs(col.g - _BlendRate);
                col.b = abs(col.b - _BlendRate);

#ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
                col.a *= m.x * m.y;
#endif

                return col;
            }
            ENDCG
        }
    }
}