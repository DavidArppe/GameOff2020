#ifndef TERRAIN_INPUTS_DRIVEN_H
#define TERRAIN_INPUTS_DRIVEN_H

#include "TerrainConstants.hlsl"

CBUFFER_START(TerrainSurfaceDrivenValues)
uint _LD_SliceIndex;
CBUFFER_END

// This must exactly match struct with same name in C#
// :CascadeParams
struct CascadeParams
{
	float2 _posSnapped;
	float _scale;
	float _textureRes;
	float _oneOverTextureRes;
	float _texelWidth;
	float _weight;
	// Align to 32 bytes
	float __padding;
};

StructuredBuffer<CascadeParams> _TerrainCascadeData;

// This must exactly match struct with same name in C#
// :PerCascadeInstanceData
struct PerCascadeInstanceData
{
	float _meshScaleLerp;
	float _farNormalsWeight;
	float _geoGridWidth;
	// Align to 32 bytes
	float _rotation;
	float4 __padding1;
};

StructuredBuffer<PerCascadeInstanceData> _TerrainPerCascadeInstanceData;

Texture2DArray _LD_TexArray_TerrainHeight;

// These are used in lods where we operate on data from
// previously calculated lods. Used in simulations and
// shadowing for example.
Texture2DArray _LD_TexArray_TerrainHeight_Source;

#endif // TERRAIN_INPUTS_DRIVEN_H
