#ifndef TERRAIN_HELPERS_H
#define TERRAIN_HELPERS_H

float2 WorldToUV(in float2 i_samplePos, in CascadeParams i_cascadeParams)
{
	return (i_samplePos - i_cascadeParams._posSnapped) / (i_cascadeParams._texelWidth * i_cascadeParams._textureRes) + 0.5;
}

float3 WorldToUV(in float2 i_samplePos, in CascadeParams i_cascadeParams, in float i_sliceIndex)
{
	float2 uv = (i_samplePos - i_cascadeParams._posSnapped) / (i_cascadeParams._texelWidth * i_cascadeParams._textureRes) + 0.5;
	return float3(uv, i_sliceIndex);
}

float2 UVToWorld(in float2 i_uv, in float i_sliceIndex, in CascadeParams i_cascadeParams)
{
	const float texelSize = i_cascadeParams._texelWidth;
	const float res = i_cascadeParams._textureRes;
	return texelSize * res * (i_uv - 0.5) + i_cascadeParams._posSnapped;
}

// Convert compute shader id to uv texture coordinates
float2 IDtoUV(in float2 i_id, in float i_width, in float i_height)
{
	return (i_id + 0.5) / float2(i_width, i_height);
}

// Sampling functions
float4 SampleDisplacements(in Texture2DArray i_dispSampler, in float3 i_uv_slice, in float i_wt, inout float3 io_worldPos)
{
	const float4 data = i_dispSampler.SampleLevel(LODData_trilinear_clamp_sampler, i_uv_slice, 0.0);
	io_worldPos += i_wt * float3(0.0f, data.r, 0.0f);
    return data.xyzw;
}

float3 SampleDisplacementsNormals(in Texture2DArray i_dispSampler, in float3 i_uv_slice, in float i_wt)
{
    return i_dispSampler.SampleLevel(LODData_trilinear_clamp_sampler, i_uv_slice, 0.0).gba;
}

void PosToSliceIndices
(
	const float2 worldXZ,
	const float minSlice,
	const float minScale,
	const float terrainScale0,
	out float slice0,
	out float slice1,
	out float lodAlpha
)
{
	const float2 offsetFromCenter = abs(worldXZ - _TerrainCenterPosWorld.xz);
	const float taxicab = max(offsetFromCenter.x, offsetFromCenter.y);
	const float radius0 = terrainScale0;
	const float sliceNumber = clamp(log2(max(taxicab / radius0, 1.0)), minSlice, _SliceCount - 1.0);

	lodAlpha = frac(sliceNumber);
	slice0 = floor(sliceNumber);
	slice1 = slice0 + 1.0;

	// lod alpha is remapped to ensure patches weld together properly. patches can vary significantly in shape (with
	// strips added and removed), and this variance depends on the base density of the mesh, as this defines the strip width.
	// using .15 as black and .85 as white should work for base mesh density as low as 16.
	const float BLACK_POINT = 0.15, WHITE_POINT = 0.85;
	lodAlpha = saturate((lodAlpha - BLACK_POINT) / (WHITE_POINT - BLACK_POINT));

	if (slice0 == 0.0)
	{
		// blend out lod0 when viewpoint gains altitude. we're using the global _MeshScaleLerp so check for LOD0 is necessary
		lodAlpha = min(lodAlpha + _MeshScaleLerp, 1.0);
	}
}

#define SampleLod(i_lodTextureArray, i_uv_slice) (i_lodTextureArray.SampleLevel(LODData_linear_clamp_sampler, i_uv_slice, 0.0))
#define SampleLodLevel(i_lodTextureArray, i_uv_slice, mips) (i_lodTextureArray.SampleLevel(LODData_linear_clamp_sampler, i_uv_slice, mips))

// Perform iteration to invert the displacement vector field - find position that displaces to query position.
float3 InvertDisplacement
(
	in const Texture2DArray i_terrainData,
	in CascadeParams i_cascadeParams,
	in uint i_sliceIndex,
	in const float3 i_positionWS,
	in const uint i_iterations
)
{
	float3 invertedDisplacedPosition = i_positionWS;
	for (uint i = 0; i < i_iterations; i++)
	{
		const float3 uv_slice = WorldToUV(invertedDisplacedPosition.xz, i_cascadeParams, i_sliceIndex);
		const float3 displacement = i_terrainData.SampleLevel(LODData_trilinear_clamp_sampler, uv_slice, 0.0).xyz;
		const float3 error = (invertedDisplacedPosition + displacement) - i_positionWS;
		invertedDisplacedPosition -= error;
	}

	return invertedDisplacedPosition;
}

#endif // TERRAIN_HELPERS_H
