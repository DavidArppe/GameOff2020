#ifndef TERRAIN_GLOBALS_H
#define TERRAIN_GLOBALS_H

SamplerState LODData_trilinear_clamp_sampler;
SamplerState LODData_point_clamp_sampler;
SamplerState sampler_linear_repeat;

CBUFFER_START(TerrainPerFrame)
float _TexelsPerPatch;
float3 _TerrainCenterPosWorld;
float _SliceCount;
float _MeshScaleLerp;
float _TerrainClipByDefault;
float _TerrainLodAlphaBlackPointFade;
float _TerrainLodAlphaBlackPointWhitePointFade;
CBUFFER_END

#endif
