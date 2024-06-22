Shader "Custom/DisintegrationShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DisintegrateProgress ("Disintegrate Progress", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DisintegrateProgress;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);

                // Disintegration effect
                float noise = frac(sin(dot(i.uv.xy, float2(12.9898, 78.233))) * 43758.5453);
                if (noise < _DisintegrateProgress)
                {
                    discard;
                }

                return texColor;
            }
            ENDCG
        }
    }
}
