using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LODDataHeightmap : LODDataManager
{
    public override string SimName { get { return "HeightMap"; } }
    public override RenderTextureFormat TextureFormat { get { return RenderTextureFormat.ARGBFloat; } }
    protected override bool NeedToReadWriteTextureData { get { return true; } }

    readonly static Color s_nullColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    static Texture2DArray s_nullTexture2DArray;

    Texture2D inputTexture;
    Material copyTerrainMat;
    public PropertyWrapperMaterial copyTerrainMatPropWrapper;

    public LODDataHeightmap(TerrainRenderer terrain) : base(terrain)
    {
        Start();

        inputTexture = Resources.Load<Texture2D>("MoonTextures/Moon_1-Heightmap");
        inputTexture.Apply();

        copyTerrainMat = new Material(Shader.Find("Unlit/CopyShader"));

        float scale = GenerateTerrainRenderTextures.Instance.sampleScale;

        copyTerrainMat.SetVector("_TexDims", new Vector4(inputTexture.width * scale, inputTexture.height * scale, 1.0f / (inputTexture.width * scale), 1.0f / (inputTexture.height * scale)));
        copyTerrainMat.SetTexture("_MainTex", inputTexture);

        copyTerrainMatPropWrapper = new PropertyWrapperMaterial(copyTerrainMat);
    }

    public override void BuildCommandBuffer(TerrainRenderer terrain, CommandBuffer buf)
    {
        base.BuildCommandBuffer(terrain, buf);

        Bind(copyTerrainMatPropWrapper);

        LODTransformTerrain lt = TerrainRenderer.Instance._lodTransform;

        for (int lodIdx = TerrainRenderer.Instance.CurrentLodCount - 1; lodIdx >= 0; lodIdx--)
        {
            buf.SetRenderTarget(_targets, 0, CubemapFace.Unknown, lodIdx);
            //buf.ClearRenderTarget(false, true, new Color(0f, 0f, 0f, 0f));
            buf.SetGlobalInt(sp_LD_SliceIndex, lodIdx);

            var lodRect = lt._renderData[lodIdx].RectXZ;

            buf.SetGlobalVector("_WorldSpaceRect", new Vector4(lodRect.xMin, lodRect.yMin, lodRect.xMax, lodRect.yMax));
            buf.Blit(null, new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive), copyTerrainMat);
        }
    }

    readonly static string s_textureArrayName = "_LD_TexArray_TerrainHeight";
    private static TextureArrayParamIds s_textureArrayParamIds = new TextureArrayParamIds(s_textureArrayName);
    public static int ParamIdSampler(bool sourceLod = false) { return s_textureArrayParamIds.GetId(sourceLod); }
    protected override int GetParamIdSampler(bool sourceLod = false)
    {
        return ParamIdSampler(sourceLod);
    }

    public static void Bind(IPropertyWrapper properties)
    {
        if (TerrainRenderer.Instance._lodDataHeightmap != null)
        {
            properties.SetTexture(TerrainRenderer.Instance._lodDataHeightmap.GetParamIdSampler(), TerrainRenderer.Instance._lodDataHeightmap.DataTexture);
        }
        else
        {
            // TextureArrayHelpers prevents use from using this in a static constructor due to blackTexture usage
            if (s_nullTexture2DArray == null)
            {
                InitNullTexture();
            }

            properties.SetTexture(ParamIdSampler(), s_nullTexture2DArray);
        }
    }

    static void InitNullTexture()
    {
        var texture = TextureArrayHelpers.CreateTexture2D(s_nullColor, UnityEngine.TextureFormat.R16);
        s_nullTexture2DArray = TextureArrayHelpers.CreateTexture2DArray(texture);
        s_nullTexture2DArray.name = "Terrain Null Texture";
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitStatics()
    {
        s_textureArrayParamIds = new TextureArrayParamIds(s_textureArrayName);
    }
}
