using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumetricClouds : MonoBehaviour
{
    [HideInInspector]
    public Material skyMaterial;
    [HideInInspector]
    public Material cloudsMaterial;
    [HideInInspector]
    public Light sun;
    [HideInInspector]
    public Transform moon;
    [HideInInspector]
    public enum CloudPerformance { Low = 0, Medium = 1, High = 2, Ultra = 3 }
    [HideInInspector]
    private int[] presetResolutions = { 1024, 2048, 2048, 2048 };
    [HideInInspector]
    private string[] keywordsA = { "LOW", "MEDIUM", "HIGH", "ULTRA" };
    [HideInInspector]
    public enum CloudType { TwoD = 0, Volumetric}
    [HideInInspector]
    private string[] keywordsB = { "TWOD", "VOLUMETRIC" };
    [HideInInspector]
    public CloudType cloudType = CloudType.Volumetric;
    [HideInInspector]
    public CloudPerformance performance = CloudPerformance.High;
    [HideInInspector]
    [Range(0, 1)] public float cloudTransparency = 0.85f;
    [HideInInspector]
    public CommandBuffer cloudsCommBuff;

    private int frameCount = 0;

    private void Start()
    {
        GenerateInitialNoise();
    }

    // Generate the random noise needed for the clouds
    void GenerateInitialNoise()
    {
        SetCloudDetails(performance, cloudType);
        //GetComponent<MeshRenderer>().enabled = true;

        GenerateNoise.GenerateBaseCloudNoise();
        GenerateNoise.GenerateCloudDetailNoise();
        GenerateNoise.GenerateCloudCurlNoise();

        GetComponent<MeshFilter>().sharedMesh = ProceduralHemispherePolarUVs.hemisphere;
        GetComponentsInChildren<MeshFilter>()[1].sharedMesh = ProceduralHemispherePolarUVs.hemisphereInv;
        skyMaterial.SetFloat("_uLightningTimer", 0.0f);

        cloudsCommBuff = new CommandBuffer();
        cloudsCommBuff.name = "Render Clouds";

        Camera.main.AddCommandBuffer(CameraEvent.AfterSkybox, cloudsCommBuff);
    }

    #region Helper Functions and Variables
    public void EnsureArray<T>(ref T[] array, int size, T initialValue = default(T))
    {
        if (array == null || array.Length != size)
        {
            array = new T[size];
            for (int i = 0; i != size; i++)
                array[i] = initialValue;
        }
    }

    public bool EnsureRenderTarget(ref RenderTexture rt, int width, int height, RenderTextureFormat format, FilterMode filterMode, string name, int depthBits = 0, int antiAliasing = 1)
    {
        if (rt != null && (rt.width != width || rt.height != height || rt.format != format || rt.filterMode != filterMode || rt.antiAliasing != antiAliasing))
        {
            RenderTexture.ReleaseTemporary(rt);
            rt = null;
        }
        if (rt == null)
        {
            rt = RenderTexture.GetTemporary(width, height, depthBits, format, RenderTextureReadWrite.Default, antiAliasing);
            rt.name = name;
            rt.filterMode = filterMode;
            rt.wrapMode = TextureWrapMode.Repeat;
            return true;// new target
        }

#if UNITY_ANDROID || UNITY_IPHONE
        rt.DiscardContents();
#endif

        return false;// same target
    }

    static int[] haltonSequence = {
        8, 4, 12, 2, 10, 6, 14, 1
    };

    static int[,] offset = {
                {2,1}, {1,2 }, {2,0}, {0,1},
                {2,3}, {3,2}, {3,1}, {0,3},
                {1,0}, {1,1}, {3,3}, {0,0},
                {2,2}, {1,3}, {3,0}, {0,2}
            };

    static int[,] bayerOffsets = {
        {0,8,2,10 },
        {12,4,14,6 },
        {3,11,1,9 },
        {15,7,13,5 }
    };
    #endregion

    private int frameIndex = 0;
    private int haltonSequenceIndex = 0;

    private int fullBufferIndex = 0;
    private RenderTexture[] fullCloudsBuffer;
    private RenderTexture lowResCloudsBuffer;

    private float baseCloudOffset;
    private float detailCloudOffset;

    public void SetCloudDetails(CloudPerformance performance, CloudType cloudType, bool forceRecreateTextures = false)
    {
        if (this.performance != performance || this.cloudType != cloudType
            || forceRecreateTextures)
        {
            if (lowResCloudsBuffer != null) lowResCloudsBuffer.Release();
            if (fullCloudsBuffer != null && fullCloudsBuffer.Length > 0)
            {
                fullCloudsBuffer[0].Release();
                fullCloudsBuffer[1].Release();
            }

            frameCount = 0;
        }

        this.performance = performance;
        this.cloudType = cloudType;

        foreach (string s in skyMaterial.shaderKeywords)
            skyMaterial.DisableKeyword(s);

        skyMaterial.EnableKeyword(keywordsA[(int)performance]);
        skyMaterial.EnableKeyword(keywordsB[(int)cloudType]);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetCloudDetails(performance, cloudType, true);
    }
#endif

    void Update()
    {
        CloudsUpdate();
    }

    void CloudsUpdate()
    {
        frameIndex = (frameIndex + 1) % 16;

        if (frameIndex == 0)
            haltonSequenceIndex = (haltonSequenceIndex + 1) % haltonSequence.Length;
        fullBufferIndex = fullBufferIndex ^ 1;

        float offsetX = offset[frameIndex, 0];
        float offsetY = offset[frameIndex, 1];

        frameCount++;
        if (frameCount < 16)
            skyMaterial.EnableKeyword("PREWARM");
        else if (frameCount == 16)
            skyMaterial.DisableKeyword("PREWARM");

        int size = presetResolutions[(int)performance];

        EnsureArray(ref fullCloudsBuffer, 2);
        EnsureRenderTarget(ref fullCloudsBuffer[0], size, size, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, "fullCloudBuff0");
        EnsureRenderTarget(ref fullCloudsBuffer[1], size, size, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, "fullCloudBuff1");
        EnsureRenderTarget(ref lowResCloudsBuffer, size / 4, size / 4, RenderTextureFormat.ARGBFloat, FilterMode.Point, "quarterCloudBuff");

        skyMaterial.SetTexture("_uBaseNoise", GenerateNoise.baseNoiseTexture);
        skyMaterial.SetTexture("_uDetailNoise", GenerateNoise.detailNoiseTexture);
        skyMaterial.SetTexture("_uCurlNoise", GenerateNoise.curlNoiseTexture);

        baseCloudOffset += skyMaterial.GetFloat("_uCloudsMovementSpeed") * Time.deltaTime;
        detailCloudOffset += skyMaterial.GetFloat("_uCloudsTurbulenceSpeed") * Time.deltaTime;

        skyMaterial.SetFloat("_uBaseCloudOffset", baseCloudOffset);
        skyMaterial.SetFloat("_uDetailCloudOffset", detailCloudOffset);

        skyMaterial.SetFloat("_uSize", size);
        skyMaterial.SetInt("_uCount", frameCount);
        skyMaterial.SetVector("_uJitter", new Vector2(offsetX, offsetY));
        skyMaterial.SetFloat("_uRaymarchOffset", (haltonSequence[haltonSequenceIndex] / 16.0f + bayerOffsets[offset[frameIndex, 0], offset[frameIndex, 1]] / 16.0f));

        skyMaterial.SetVector("_uSunDir", sun.transform.forward);
        skyMaterial.SetVector("_uMoonDir", Vector3.Normalize(moon.forward));
        skyMaterial.SetVector("_uWorldSpaceCameraPos", Camera.main.transform.position);

        #region Command Buffer
        cloudsCommBuff.Clear();

        // 1. Render the first clouds buffer - lower resolution
        cloudsCommBuff.Blit(null, lowResCloudsBuffer, skyMaterial, 0);

        // 2. Blend between low and hi-res
        cloudsCommBuff.SetGlobalTexture("_uLowresCloudTex", lowResCloudsBuffer);
        cloudsCommBuff.SetGlobalTexture("_uPreviousCloudTex", fullCloudsBuffer[fullBufferIndex]);
        cloudsCommBuff.Blit(fullCloudsBuffer[fullBufferIndex], fullCloudsBuffer[fullBufferIndex ^ 1], skyMaterial, 1);

        cloudsCommBuff.SetGlobalFloat("_uLightning", 0.0f);
        #endregion

        // 3. Set to material for the sky (not in the command buffer)
        cloudsMaterial.SetTexture("_MainTex", fullCloudsBuffer[fullBufferIndex ^ 1]);
    }
}
 