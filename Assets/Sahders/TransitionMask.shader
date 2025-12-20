Shader "UI/TransitionMask"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Range ("Transition Range", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float _Range;
            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Get the Main Color (Background)
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
                
                // Get the Mask Color (Greyscale)
                fixed4 mask = tex2D(_MaskTex, i.texcoord);
                
                // Logic: If Mask brightness < Range, it becomes transparent
                // In Steins;Gate, masks define the transition pattern.
                // We use step function for hard cutout, or smoothstep for soft edge.
                
                // If _Range is 1, fully visible. If 0, fully invisible (depending on mask).
                // Let's assume standard Dissolve logic:
                float alpha = step(1 - _Range, mask.r);
                
                col.a *= alpha;
                
                return col;
            }
            ENDCG
        }
    }
}