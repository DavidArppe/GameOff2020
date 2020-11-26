// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MADSOCK/ShieldShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "black" {}
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _UColor("Under Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Power("Texture Power", float) = 1.0
        _HoneyCombScale("Honeycomb Scale", Vector) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "RenderType" = "Overlay+5000" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Off
        ZWrite Off
        ZTest On

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _UColor;
            float _Power;
            float4 _HoneyCombScale;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 SP = float2(1.0, 1.0);

                float2 uv = (i.uv * 1.0) / SP.xy;

                float t = _Time.y * 5.0;
                float2 p = 2.0 * uv - 1.0;
                float2 op = p;
                p.x *= SP.x / SP.y;
                float3 color = float3(0,0,0);
                float v = length(2.0*uv - 1.0);

                p *= 100.0;

                float movex = sin(t + p.x + p.y*v);
                float movey = sin(t*0.5);
                float grid = sin(p.x + movex)*sin(p.y + movey);
                float grid2 = grid + 0.2;
                float grid3 = grid + 0.3;

                float inner = 1.0 - ceil(grid + grid2);
                float outer = 1.0 - ceil(grid + grid3);

                inner = max(0.0,min(1.0,inner));
                outer = max(0.0,min(1.0,outer));
                float stencil = inner - outer;

                color.r = inner - outer;
                p *= 0.5;
                p = abs(p);
                p *= v;
                p.x += sin(p.y*sin(p.x));
                float beam = sin(p.x + t);
                beam = max(0.0,min(1.0,beam));

                color.r += beam;
                color.r *= beam * grid;
                color *= 2.0;
                color.r += stencil * beam;
                color.r = max(0.0,min(1.0,color.r));
                color.g = color.r*0.5;

                // high constrast and vinj
                color *= color * 10.0;
                color *= 1.0 - v;

                //output final color
                float3 luma = float3(0.299, 0.587, 0.114);
                float alpha = dot(luma, color);

                float4 col = float4(_Color.x * alpha, _Color.y * alpha, _Color.z * alpha, alpha * _Color.a);

                col.rgba = lerp(_UColor.rgba, col.rgba, alpha);
                col.rgb *= float3(1.0f, 1.0f, 1.0f) - (tex2D(_MainTex, i.uv * _HoneyCombScale.xy + _HoneyCombScale.zw).rgb * _Power);
                col.rgba *= smoothstep(400.0f, 300.0f, length(_WorldSpaceCameraPos - i.worldPos));
                return col;
            }
            ENDCG
        }
    }
}