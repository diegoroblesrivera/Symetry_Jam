Shader "Custom/Inky_Color"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color0 ("Mas Claro (Blanco)", Color) = (0.6, 0.7, 0.1, 1)
        _Color1 ("Gris Claro", Color) = (0.4, 0.5, 0.1, 1)
        _Color2 ("Gris Oscuro", Color) = (0.1, 0.3, 0.1, 1)
        _Color3 ("Mas Oscuro (Negro)", Color) = (0.0, 0.1, 0.0, 1)
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
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color0, _Color1, _Color2, _Color3;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                // Usamos el valor del canal Rojo como índice (0 a 1)
                float index = tex.r;

                fixed4 finalCol;
                
                // Dividimos el rango en 4 secciones
                if (index > 0.8)      finalCol = _Color0;
                else if (index > 0.5) finalCol = _Color1;
                else if (index > 0.2) finalCol = _Color2;
                else                  finalCol = _Color3;

                // Aplicar el Alpha original y el color del SpriteRenderer (para parpadeos)
                finalCol.a *= tex.a * i.color.a;
                return finalCol;
            }
            ENDCG
        }
    }
}