Shader "Planetary Tessellation" 
{
    Properties
    {
        _EdgeLength("Edge length", Range(2,50)) = 15
        _MainTex("Base (RGB)", 2D) = "white" {}
        _DispTex("Disp Texture", 2D) = "gray" {}
        _NormalMap("Normalmap", 2D) = "bump" {}
        _Displacement("Displacement", Range(0, 1.0)) = 0.3
        _UVScale("UV Scale", Float) = 1.0
        _NormalsStrength("Normals Scale", Float) = 1.0
        _Color("Color", color) = (1,1,1,0)
        _SpecColor("Spec color", color) = (0.5,0.5,0.5,0.5)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:disp tessellate:tessEdge nolightmap
        #pragma target 4.6
        #include "Tessellation.cginc"

        struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
        };

        float _EdgeLength;

        float4 tessEdge(appdata v0, appdata v1, appdata v2)
        {
            return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
        }

        sampler2D _DispTex;
        float _UVScale;
        float _Displacement;
        float _NormalsStrength;

        float3 normalsFromHeight(sampler2D heightTex, float2 uv, float texelSize, float strength)
        {
            float4 h;
            float rcpTexSize = 1 / 2048.0f;

            h[0] = tex2Dlod(heightTex, float4(uv, 0.0f, 0.0f) + float4(texelSize * float2(0, -rcpTexSize), 0, 0)).r * strength;
            h[1] = tex2Dlod(heightTex, float4(uv, 0.0f, 0.0f) + float4(texelSize * float2(-rcpTexSize, 0), 0, 0)).r * strength;
            h[2] = tex2Dlod(heightTex, float4(uv, 0.0f, 0.0f) + float4(texelSize * float2(rcpTexSize, 0), 0, 0)).r * strength;
            h[3] = tex2Dlod(heightTex, float4(uv, 0.0f, 0.0f) + float4(texelSize * float2(0, rcpTexSize), 0, 0)).r * strength;

            float3 n;
            n.z = h[3] - h[0];
            n.x = h[2] - h[1];
            n.y = 2;
            return normalize(n);
        }
        
        void disp(inout appdata v)
        {
            float3 vertexWP = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 viewDir = normalize(_WorldSpaceCameraPos - vertexWP);
            float strength = pow(dot(viewDir, v.normal), 0.25f);

            float d = tex2Dlod(_DispTex, float4(v.vertex.xz * _UVScale,0,0)).r * _Displacement * strength * 0.01f * smoothstep(0.075f, 0.09f, length(v.vertex.xz));
            v.vertex.xyz += v.normal * d;
            //v.normal = normalsFromHeight(_DispTex, v.vertex.xz * _UVScale, 1.0f, strength * _NormalsStrength);
        }

        struct Input {
            float3 worldPos;
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        sampler2D _NormalMap;
        fixed4 _Color;
        
        void surf(Input IN, inout SurfaceOutput o) 
        {
            half4 c = tex2D(_MainTex, IN.uv_MainTex * _UVScale) * _Color;
            o.Albedo = c.rgb;
            o.Specular = 0.2;
            o.Gloss = 1.0;

            float3 n = normalsFromHeight(_DispTex, (IN.worldPos.xz / 100000) * _UVScale, 1.0f, _NormalsStrength);

            o.Normal = n.rbg;
            //o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex * _UVScale));
        }
        ENDCG
    }
    FallBack "Diffuse"
}