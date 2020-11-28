Shader "MADSOCK/TerrainSurf"
{
    Properties
    {
		[Header(Debug Options)]
		[Toggle] _CompileShaderWithDebugInfo("Compile Shader With Debug Info (D3D11)", Float) = 0

        _BayerTex("Bayer Mat", 2D) = "white" {}

        [Header(Main Material)]
        _Color("Color", Color) = ( 0.0, 0.0, 0.0, 0.0 )
        _MainTex("Base Color", 2D) = "white" {}
        _Normals("Normals", 2D) = "bump" {}
        _Metallic("Metallic", 2D) = "black" {}
        _AO("AO", 2D) = "white" {}
        _Height("Height", 2D) = "black" {}
        _BumpScale("Bump Scale", Float) = 1.0
        _Displacement("Displacement", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard vertex:vert fullforwardshadows addshadow nolightmap
        #pragma target 3.0

		#pragma shader_feature_local _COMPILESHADERWITHDEBUGINFO_ON

#if _COMPILESHADERWITHDEBUGINFO_ON
		#pragma enable_d3d11_debug_symbols
#endif

		#include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _Normals;
        float _BumpScale;
        float _Displacement;
        sampler2D _Metallic;
        sampler2D _AO;
        sampler2D _Height;
        float4 _Color;

        struct Input
        {
            float4 lodAlpha_worldXZUndisplaced;
            float4 customWorldPos; // can't name it just worldPos
            float3 worldNormal;
            float4 customScreenPos;
        };

#ifdef SHADER_API_D3D11
        #include "TerrainConstants.hlsl"
        #include "TerrainGlobals.hlsl"
        #include "TerrainInputsDriven.hlsl"
        #include "TerrainHelpersNew.hlsl"
        #include "TerrainVertHelpers.hlsl"
#endif

        float4 hash4(float2 p) {
            return frac(sin(float4(1.0 + dot(p, float2(37.0, 17.0)),
                2.0 + dot(p, float2(11.0, 47.0)),
                3.0 + dot(p, float2(41.0, 29.0)),
                4.0 + dot(p, float2(23.0, 31.0))))*103.0);
        }

        // Prevent texture repetition (expensiveish - 4 texture lookups)
        // http://www.iquilezles.org/www/articles/texturerepetition/texturerepetition.htm
        float4 TextureNoTile(sampler2D samp, in float2 uv)
        {
            int2 iuv = int2(floor(uv));
            float2 fuv = frac(uv);

            // generate per-tile transform
            float4 ofa = hash4(iuv + int2(0, 0));
            float4 ofb = hash4(iuv + int2(1, 0));
            float4 ofc = hash4(iuv + int2(0, 1));
            float4 ofd = hash4(iuv + int2(1, 1));

            float2 _ddx = ddx(uv);
            float2 _ddy = ddy(uv);

            // transform per-tile uvs
            ofa.zw = sign(ofa.zw - 0.5);
            ofb.zw = sign(ofb.zw - 0.5);
            ofc.zw = sign(ofc.zw - 0.5);
            ofd.zw = sign(ofd.zw - 0.5);

            // uv's, and derivatives (for correct mipmapping)
            float2 uva = uv * ofa.zw + ofa.xy, ddxa = _ddx * ofa.zw, ddya = _ddy * ofa.zw;
            float2 uvb = uv * ofb.zw + ofb.xy, ddxb = _ddx * ofb.zw, ddyb = _ddy * ofb.zw;
            float2 uvc = uv * ofc.zw + ofc.xy, ddxc = _ddx * ofc.zw, ddyc = _ddy * ofc.zw;
            float2 uvd = uv * ofd.zw + ofd.xy, ddxd = _ddx * ofd.zw, ddyd = _ddy * ofd.zw;

            // fetch and blend
            float2 b = smoothstep(0.25, 0.75, fuv);

            return lerp(lerp(tex2Dgrad(samp, uva, ddxa, ddya),
                tex2Dgrad(samp, uvb, ddxb, ddyb), b.x),
                lerp(tex2Dgrad(samp, uvc, ddxc, ddyc),
                    tex2Dgrad(samp, uvd, ddxd, ddyd), b.x), b.y);
        }

        // Prevent texture repetition (expensiveish - 4 texture lookups)
       // http://www.iquilezles.org/www/articles/texturerepetition/texturerepetition.htm
        float4 TextureNoTileLOD(sampler2D samp, in float2 uv)
        {
            int2 iuv = int2(floor(uv));
            float2 fuv = frac(uv);

            // generate per-tile transform
            float4 ofa = hash4(iuv + int2(0, 0));
            float4 ofb = hash4(iuv + int2(1, 0));
            float4 ofc = hash4(iuv + int2(0, 1));
            float4 ofd = hash4(iuv + int2(1, 1));

            float2 _ddx = ddx(uv);
            float2 _ddy = ddy(uv);

            // transform per-tile uvs
            ofa.zw = sign(ofa.zw - 0.5);
            ofb.zw = sign(ofb.zw - 0.5);
            ofc.zw = sign(ofc.zw - 0.5);
            ofd.zw = sign(ofd.zw - 0.5);

            // uv's, and derivatives (for correct mipmapping)
            float2 uva = uv * ofa.zw + ofa.xy, ddxa = _ddx * ofa.zw, ddya = _ddy * ofa.zw;
            float2 uvb = uv * ofb.zw + ofb.xy, ddxb = _ddx * ofb.zw, ddyb = _ddy * ofb.zw;
            float2 uvc = uv * ofc.zw + ofc.xy, ddxc = _ddx * ofc.zw, ddyc = _ddy * ofc.zw;
            float2 uvd = uv * ofd.zw + ofd.xy, ddxd = _ddx * ofd.zw, ddyd = _ddy * ofd.zw;

            // fetch and blend
            float2 b = smoothstep(0.25, 0.75, fuv);

            return lerp(lerp(tex2Dlod(samp, float4(uva, 0.0f, 0.0f)),
                tex2Dlod(samp, float4(uvb, 0.0f, 0.0f)), b.x),
                lerp(tex2Dlod(samp, float4(uvc, 0.0f, 0.0f)),
                    tex2Dlod(samp, float4(uvd, 0.0f, 0.0f)), b.x), b.y);
        }

        #define UV_SCALE 0.01f

#ifdef SHADER_API_D3D11
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            // Scale up by small "epsilon" to solve numerical issues.
			v.vertex.xyz *= 1.00001;

			const CascadeParams cascadeData0 = _TerrainCascadeData[_LD_SliceIndex];
			const CascadeParams cascadeData1 = _TerrainCascadeData[_LD_SliceIndex + 1];
			const PerCascadeInstanceData instanceData = _TerrainPerCascadeInstanceData[_LD_SliceIndex];

			// Move to world space
			o.customWorldPos.xyz = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f)).xyz;

			// Vertex snapping and lod transition
			float lodAlpha = 0.0f;
			const float meshScaleLerp = instanceData._meshScaleLerp;
			const float gridSize = instanceData._geoGridWidth;
			SnapAndTransitionVertLayout(meshScaleLerp, cascadeData0, gridSize, o.customWorldPos.xyz, lodAlpha);
			o.lodAlpha_worldXZUndisplaced.x = lodAlpha;
			o.lodAlpha_worldXZUndisplaced.yz = o.customWorldPos.xz;
				
			// Calculate sample weights. params.z allows shape to be faded out (used on last lod to support pop-less scale transitions)
			const float wt_smallerLod = (1. - lodAlpha) * cascadeData0._weight;
			const float wt_biggerLod = (1. - wt_smallerLod) * cascadeData1._weight;
			// Sample displacement textures, add results to current world pos / normal / foam
			const float2 positionWS_XZ_before = o.customWorldPos.xz;

            float3 vertNorm = 0;
			if (wt_smallerLod > 0.001)
			{
				const float3 uv_slice_smallerLod = WorldToUV(positionWS_XZ_before, _TerrainCascadeData[_LD_SliceIndex], _LD_SliceIndex);
                vertNorm = SampleDisplacements(_LD_TexArray_TerrainHeight, uv_slice_smallerLod, wt_smallerLod * 200.0f, o.customWorldPos.xyz).yzw;
			}
			if (wt_biggerLod > 0.001)
			{
                const uint si = _LD_SliceIndex + 1;
				const float3 uv_slice_biggerLod = WorldToUV(positionWS_XZ_before, _TerrainCascadeData[si], si);
                vertNorm = SampleDisplacements(_LD_TexArray_TerrainHeight, uv_slice_biggerLod, wt_biggerLod * 200.0f, o.customWorldPos.xyz).yzw;
			}

			// view-projection
            float4 clipPos = mul(UNITY_MATRIX_VP, float4(o.customWorldPos.xyz, 1.0f));
			o.customWorldPos.w = clipPos.z;
            o.customScreenPos = ComputeScreenPos(clipPos);
            
            float d         = TextureNoTileLOD(_Height, float2(o.customWorldPos.xz * UV_SCALE)).r;
            v.vertex.xyz    = mul(unity_WorldToObject, float4(o.customWorldPos.xyz + float3(0.0f, d * _Displacement, 0.0f), 1.0f));
            v.texcoord      = o.customWorldPos.xzxz;
        }
#else
        void vert(inout appdata_full v, out Input o)
        {
            // Stub function if not supported graphics API
        }
#endif

#ifdef SHADER_API_D3D11
        uniform sampler2D _BayerTex; // the generated texture

        float remap(float low1, float high1, float low2, float high2, float value)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }

        float Bayer(float2 uv)
        {
            return tex2D(_BayerTex, uv / 8.0f).r;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            if (length(IN.customWorldPos.xz) > 8192.0f) discard;
            
            float2 screenUV = IN.customScreenPos.xy / max(0.0001f, IN.customScreenPos.w);
            if (Bayer(screenUV * _ScreenParams.xy) < remap(6000.0f, 8192.0f, 0.0f, 1.0f, length(IN.customWorldPos.xz))) discard;
            
            const CascadeParams cascadeData0 = _TerrainCascadeData[_LD_SliceIndex];
            const CascadeParams cascadeData1 = _TerrainCascadeData[_LD_SliceIndex + 1];
            const PerCascadeInstanceData instanceData = _TerrainPerCascadeInstanceData[_LD_SliceIndex];

            const float lodAlpha = IN.lodAlpha_worldXZUndisplaced.x;
            const float wt_smallerLod = (1.0 - lodAlpha) * cascadeData0._weight;
            const float wt_biggerLod = (1.0 - wt_smallerLod) * cascadeData1._weight;
            
            float3 norm = float3(0.0, 1.0, 0.0);
            if (wt_smallerLod > 0.001)
            {
                const float3 uv_slice_smallerLod = WorldToUV(IN.lodAlpha_worldXZUndisplaced.yz, _TerrainCascadeData[_LD_SliceIndex], _LD_SliceIndex);
                norm = SampleDisplacementsNormals(_LD_TexArray_TerrainHeight, uv_slice_smallerLod, wt_smallerLod);
            }
            if (wt_biggerLod > 0.001)
            {
                const uint si = _LD_SliceIndex + 1;
                const float3 uv_slice_biggerLod = WorldToUV(IN.lodAlpha_worldXZUndisplaced.yz, _TerrainCascadeData[si], si);
                norm = SampleDisplacementsNormals(_LD_TexArray_TerrainHeight, uv_slice_biggerLod, wt_biggerLod);
            }
            norm = normalize(norm);

            float2 uvs = IN.customWorldPos.xz * UV_SCALE;
            float4 metallicRGBSmoothnessA = TextureNoTile(_Metallic, uvs);

            float3 normal = norm.rgb * 2.0f - 1.0f;
            float3 tangent = cross(norm, float3(0,1,0));
            float3 bitangent = cross(norm, tangent);

            float3 unpackedNorms = UnpackScaleNormal(TextureNoTile(_Normals, uvs), _BumpScale);

            o.Smoothness = metallicRGBSmoothnessA.a * 0.4;
            o.Metallic = metallicRGBSmoothnessA.r;
            o.Normal = normalize(tangent * -unpackedNorms.x + normal * unpackedNorms.z + bitangent * unpackedNorms.y);
            o.Albedo = TextureNoTile(_MainTex, uvs).rgb * _Color.rgb * TextureNoTile(_AO, uvs).r;
            o.Alpha = 1.0f;
        }
#else
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = float3(1.0f, 1.0f, 1.0f);
            o.Alpha = 1.0f;
        }
#endif
        ENDCG
    }
    FallBack "Diffuse"
}
