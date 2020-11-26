Shader "Planetary Tessellation" 
{
    Properties
    {
        _Color("Color", Color) = (0.0, 0.0, 0.0, 0.0)
        _MainTex("Base Color", 2D) = "white" {}
        _Normals("Normals", 2D) = "bump" {}
        _Metallic("Metallic", 2D) = "black" {}
        _AO("AO", 2D) = "white" {}

        _BumpScaleBigNorms("Bump Scale Big Norms", Float) = 20
        _BumpScale("Bump Scale", Float) = 1.0
        _EdgeLength("Edge length", Range(2,50)) = 15
        _DispTex("Disp Texture", 2D) = "black" {}
        _Displacement("Displacement", Range(0, 1.0)) = 0.3
        _BaseHeightOffset("Height Bias", Range(-2.0, 2.0)) = 0.0
        _InnerOuter("Inner/Outer", Vector) = (0.075, 0.09, 0.0, 0.0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard vertex:disp tessellate:tessEdge nolightmap addshadow fullforwardshadows
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

        #define UV_SCALE 0.01f
        #define BIG_UV_SCALE 0.00007f

        sampler2D _DispTex;
        float _UVScale;
        float _Displacement;
        float _BumpScale;
        float _BumpScaleBigNorms;
        sampler2D _MainTex;
        sampler2D _Normals;
        sampler2D _Metallic;
        sampler2D _AO;
        float4 _Color;
        float _BaseHeightOffset;
        float2 _InnerOuter;

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

            float d = tex2Dlod(_DispTex, float4(vertexWP.xz * BIG_UV_SCALE,0,0)).r * _Displacement * strength * 0.01f * smoothstep(_InnerOuter.x, _InnerOuter.y, length(v.vertex.xz));
            d += _BaseHeightOffset * smoothstep(_InnerOuter.x, _InnerOuter.y, length(v.vertex.xz));
            v.vertex.xyz += v.normal * d;
        }

        struct Input {
            float3 worldPos;
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o) 
        {
            float2 uvs = IN.worldPos.xz * UV_SCALE;

            float3 norm = normalsFromHeight(_DispTex, (IN.worldPos.xz * BIG_UV_SCALE), 1.0f, _BumpScaleBigNorms);
            float4 metallicRGBSmoothnessA = tex2D(_Metallic, uvs);

            float3 normal = norm.rgb;// * 2.0f - 1.0f;
            float3 tangent = cross(norm, float3(0, 1, 0));
            float3 bitangent = cross(norm, tangent);

            float3 unpackedNorms = UnpackScaleNormal(tex2D(_Normals, uvs), _BumpScale);

            o.Smoothness = metallicRGBSmoothnessA.a * 0.4;
            o.Metallic = metallicRGBSmoothnessA.r;
            o.Normal = normalize(tangent * -unpackedNorms.x + normal * unpackedNorms.z + bitangent * unpackedNorms.y);
            o.Albedo = tex2D(_MainTex, uvs).rgb * _Color.rgb * tex2D(_AO, uvs).r;
            o.Alpha = 1.0f;
        }
        ENDCG
    }
    FallBack "Diffuse"
}