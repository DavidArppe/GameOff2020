using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways, SelectionBase]
public class TerrainRenderer : MonoBehaviour
{
    [Tooltip("The viewpoint which drives the ocean detail. Defaults to the camera."), SerializeField]
    Transform _viewpoint;
    public Transform Viewpoint
    {
        get
        {
#if UNITY_EDITOR
            if (_followSceneCamera)
            {
                var sceneViewCamera = EditorHelpers.GetActiveSceneViewCamera();
                if (sceneViewCamera != null)
                {
                    return sceneViewCamera.transform;
                }
            }
#endif
            if (_viewpoint != null)
            {
                return _viewpoint;
            }

            // Even with performance improvements, it is still good to cache whenever possible.
            var camera = ViewCamera;

            if (camera != null)
            {
                return camera.transform;
            }

            return null;
        }
        set
        {
            _viewpoint = value;
        }
    }

    [Tooltip("The camera which drives the ocean data. Defaults to main camera."), SerializeField]
    Camera _camera;
    public Camera ViewCamera
    {
        get
        {
#if UNITY_EDITOR
            if (_followSceneCamera)
            {
                var sceneViewCamera = EditorHelpers.GetActiveSceneViewCamera();
                if (sceneViewCamera != null)
                {
                    return sceneViewCamera;
                }
            }
#endif

            if (_camera != null)
            {
                return _camera;
            }

            // Unity has greatly improved performance of this operation in 2019.4.9.
            return Camera.main;
        }
        set
        {
            _camera = value;
        }
    }

    public static int FrameCount
    {
        get
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return _editorFrames;
            }
            else
#endif
            {
                return Time.frameCount;
            }
        }
    }

    /// <summary>
    /// The number of LODs/scales that the terrain is currently using.
    /// </summary>
    public int CurrentLodCount { get { return _lodTransform != null ? _lodTransform.LodCount : 0; } }

    /// <summary>
    /// The terrain changes scale when viewer changes altitude, this gives the interpolation param between scales.
    /// </summary>
    public float ViewerAltitudeLevelAlpha { get; private set; }

    /// <summary>
    /// Current terrain scale (changes with viewer altitude).
    /// </summary>
    public float Scale { get; private set; }
    public float CalcLodScale(float lodIndex) { return Scale * Mathf.Pow(2f, lodIndex); }
    public float CalcGridSize(int lodIndex) { return CalcLodScale(lodIndex) / LodDataResolution; }

    [Range(2, 16)]
    [Tooltip("Min number of verts / shape texels per wave."), SerializeField]
    float _minTexelsPerWave = 3f;
    public float MinTexelsPerWave => _minTexelsPerWave;

    // We are computing these values to be optimal based on the base mesh vertex density.
    float _lodAlphaBlackPointFade;
    float _lodAlphaBlackPointWhitePointFade;

    readonly int sp_texelsPerWave = Shader.PropertyToID("_TexelsPerPatch");
    readonly int sp_meshScaleLerp = Shader.PropertyToID("_MeshScaleLerp");
    readonly int sp_sliceCount = Shader.PropertyToID("_SliceCount");
    readonly int sp_TerrainCenterPosWorld = Shader.PropertyToID("_TerrainCenterPosWorld");
    readonly int sp_lodAlphaBlackPointFade = Shader.PropertyToID("_TerrainLodAlphaBlackPointFade");
    readonly int sp_lodAlphaBlackPointWhitePointFade = Shader.PropertyToID("_TerrainLodAlphaBlackPointWhitePointFade");
    public static int sp_perCascadeInstanceData = Shader.PropertyToID("_TerrainPerCascadeInstanceData");
    public static int sp_cascadeData = Shader.PropertyToID("_TerrainCascadeData");

    BuildCommandBuffer _commandbufferBuilder;

    public static TerrainRenderer Instance { get; private set; }


    // This must exactly match struct with same name in HLSL
    // :CascadeParams
    public struct CascadeParams
    {
        public Vector2 _posSnapped;
        public float _scale;

        public float _textureRes;
        public float _oneOverTextureRes;

        public float _texelWidth;

        public float _weight;

        // Align to 32 bytes
        public float __padding;
    }
    public ComputeBuffer _bufCascadeDataTgt;
    public ComputeBuffer _bufCascadeDataSrc;

    // This must exactly match struct with same name in HLSL
    // :PerCascadeInstanceData
    public struct PerCascadeInstanceData
    {
        public float _meshScaleLerp;
        public float _farNormalsWeight;
        public float _geoGridWidth;

        // Align to 32 bytes
        public float _rotation;
        public Vector4 __padding1;
    }
    public ComputeBuffer _bufPerCascadeInstanceData;

    CascadeParams[] _cascadeParamsSrc = new CascadeParams[LODDataManager.MAX_LOD_COUNT + 1];
    CascadeParams[] _cascadeParamsTgt = new CascadeParams[LODDataManager.MAX_LOD_COUNT + 1];

    PerCascadeInstanceData[] _perCascadeInstanceData = new PerCascadeInstanceData[LODDataManager.MAX_LOD_COUNT];

    [SerializeField]
    string _layerName = "Terrain";
    public string LayerName { get { return _layerName; } }

    [SerializeField, Delayed, Tooltip("Resolution of ocean LOD data. Use even numbers like 256 or 384. This is 4x the old 'Base Vert Density' param, so if you used 64 for this param, set this to 256. Press 'Rebuild Terrain' button below to apply.")]
    int _lodDataResolution = 256;
    public int LodDataResolution { get { return _lodDataResolution; } }

    [SerializeField, Delayed, Tooltip("How much of the terrain shape gets tessellated by geometry. If set to e.g. 4, every geometry quad will span 4x4 LOD data texels. Use power of 2 values like 1, 2, 4... Press 'Rebuild Terrain' button below to apply.")]
    int _geometryDownSampleFactor = 2;

    [SerializeField, Tooltip("Number of terrain tile scales/LODs to generate. Press 'Rebuild Terrain' button below to apply."), Range(2, LODDataManager.MAX_LOD_COUNT)]
    int _lodCount = 7;

    public Transform Root { get; private set; }
    List<TerrainChunkRenderer> _terrainChunkRenderers = new List<TerrainChunkRenderer>();

    /// <summary>
    /// Sea level is given by y coordinate of GameObject with TerrainRenderer script.
    /// </summary>
    public float SeaLevel { get { return Root.position.y; } }

    [HideInInspector] public LODTransformTerrain _lodTransform;
    [HideInInspector] public LODDataHeightmap _lodDataHeightmap;

    List<LODDataManager> _lodDatas = new List<LODDataManager>();

    [Delayed, Tooltip("The smallest scale the terrain can be."), SerializeField]
    float _minScale = 8f;

    [Delayed, Tooltip("The largest scale the terrain can be (-1 for unlimited)."), SerializeField]
    float _maxScale = 256f;

    [SerializeField, Tooltip("Material to use for the terrain surface")]
    Material _material = null;
    public Material TerrainMaterial { get { return _material; } }

    [Tooltip("Set the ocean surface tiles hidden by default to clean up the hierarchy.")]
    public bool _hideTerrainTileGameObjects = true;
    [HideInInspector, Tooltip("Whether to generate ocean geometry tiles uniformly (with overlaps).")]
    public bool _uniformTiles = false;
    [Tooltip("Disable generating a wide strip of triangles at the outer edge to extend ocean to edge of view frustum.")]
    public bool _disableSkirt = false;

    [Tooltip("Move terrain with viewpoint.")]
    bool _followViewpoint = true;

#if UNITY_EDITOR
    static float _lastUpdateEditorTime = -1f;
    public static float LastUpdateEditorTime => _lastUpdateEditorTime;
    static int _editorFrames = 0;
#endif

    [Tooltip("Sets the update rate of the ocean system when in edit mode. Can be reduced to save power."), Range(0f, 60f), SerializeField]
#pragma warning disable 414
    float _editModeFPS = 30f;
#pragma warning restore 414

    [Tooltip("Move terrain with Scene view camera if Scene window is focused."), SerializeField]
#pragma warning disable 414
    bool _followSceneCamera = true;
#pragma warning restore 414

    static float _maxVertDispFromShape = 200f;
    int _maxDisplacementCachedTime = 0;

    /// <summary>
    /// The maximum height that the shape scripts are displacing the shape.
    /// </summary>
    public float MaxVertDisplacement { get { return _maxVertDispFromShape; } }

    void OnEnable()
    {
        // We don't run in "prefab scenes", i.e. when editing a prefab. Bail out if prefab scene is detected.
#if UNITY_EDITOR
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            return;
        }
#endif

        if (!VerifyRequirements())
        {
            enabled = false;
            return;
        }

#if UNITY_EDITOR
        if (EditorApplication.isPlaying && !Validate(this))
        {
            enabled = false;
            return;
        }
#endif

        Instance = this;
        Scale = Mathf.Clamp(Scale, _minScale, _maxScale);

        _bufPerCascadeInstanceData = new ComputeBuffer(_perCascadeInstanceData.Length, UnsafeUtility.SizeOf<PerCascadeInstanceData>());
        Shader.SetGlobalBuffer("_TerrainPerCascadeInstanceData", _bufPerCascadeInstanceData);

        _bufCascadeDataTgt = new ComputeBuffer(_cascadeParamsTgt.Length, UnsafeUtility.SizeOf<CascadeParams>());
        Shader.SetGlobalBuffer(sp_cascadeData, _bufCascadeDataTgt);

        // Not used by graphics shaders, so not set globally (global does not work for compute)
        _bufCascadeDataSrc = new ComputeBuffer(_cascadeParamsSrc.Length, UnsafeUtility.SizeOf<CascadeParams>());

        _lodTransform = new LODTransformTerrain();
        _lodTransform.InitLODData(_lodCount);

        // Resolution is 4 tiles across.
        var baseMeshDensity = _lodDataResolution * 0.25f / _geometryDownSampleFactor;
        // 0.4f is the "best" value when base mesh density is 8. Scaling down from there produces results similar to
        // hand crafted values which looked good when the ocean is flat.
        _lodAlphaBlackPointFade = 0.4f / (baseMeshDensity / 8f);
        // We could calculate this in the shader, but we can save two subtractions this way.
        _lodAlphaBlackPointWhitePointFade = 1f - _lodAlphaBlackPointFade - _lodAlphaBlackPointFade;

        Root = TerrainBuilder.GenerateMesh(this, _terrainChunkRenderers, _lodDataResolution, _geometryDownSampleFactor, _lodCount);

        _commandbufferBuilder = new BuildCommandBuffer();

        ValidateViewpoint();

#if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
        EditorApplication.update += EditorUpdate;
#endif

        foreach (LODDataManager lodData in _lodDatas)
        {
            lodData.OnEnable();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitStatics()
    {
        Debug.Log("Terrain Renderer Init Statics");

        // Init here from 2019.3 onwards
        Instance = null;

        // TODO: Heightmap in here
        sp_perCascadeInstanceData = Shader.PropertyToID("_TerrainPerCascadeInstanceData");
        sp_cascadeData = Shader.PropertyToID("_TerrainCascadeData");
    }

#if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnReLoadScripts()
    {
        Instance = FindObjectOfType<TerrainRenderer>();
    }
#endif

    private void OnDisable()
    {
#if UNITY_EDITOR
        // We don't run in "prefab scenes", i.e. when editing a prefab. Bail out if prefab scene is detected.
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            return;
        }
#endif

        CleanUp();

        Instance = null;
    }

    void LateUpdate()
    {
#if UNITY_EDITOR
        // Don't run immediately if in edit mode - need to count editor frames so this is run through EditorUpdate()
        if (!EditorApplication.isPlaying)
        {
            return;
        }
#endif

        RunUpdate();
    }

#if UNITY_EDITOR
    static void EditorUpdate()
    {
        if (Instance == null) return;

        if (!EditorApplication.isPlaying)
        {
            if (EditorApplication.timeSinceStartup - _lastUpdateEditorTime > 1f / Mathf.Clamp(Instance._editModeFPS, 0.01f, 60f))
            {
                _editorFrames++;

                _lastUpdateEditorTime = (float)EditorApplication.timeSinceStartup;

                Instance.RunUpdate();
            }
        }
    }
#endif

    void RunUpdate()
    {
        Shader.SetGlobalFloat(sp_texelsPerWave, MinTexelsPerWave);
        Shader.SetGlobalFloat(sp_sliceCount, CurrentLodCount);
        Shader.SetGlobalFloat(sp_lodAlphaBlackPointFade, _lodAlphaBlackPointFade);
        Shader.SetGlobalFloat(sp_lodAlphaBlackPointWhitePointFade, _lodAlphaBlackPointWhitePointFade);

        // LOD 0 is blended in/out when scale changes, to eliminate pops. Here we set it as a global, whereas in OceanChunkRenderer it
        // is applied to LOD0 tiles only through instance data. This global can be used in compute, where we only apply this factor for slice 0.
        var needToBlendOutShape = ScaleCouldIncrease;
        var meshScaleLerp = needToBlendOutShape ? ViewerAltitudeLevelAlpha : 0f;
        Shader.SetGlobalFloat(sp_meshScaleLerp, meshScaleLerp);

        ValidateViewpoint();

        if (_followViewpoint && Viewpoint != null)
        {
            LateUpdatePosition();
            LateUpdateScale();
        }

        CreateDestroySubSystems();

        LateUpdateLods();

        WritePerFrameMaterialParams();


// #if UNITY_EDITOR
//         if (EditorApplication.isPlaying) // TODO? || !_showOceanProxyPlane)
// #endif
        {
            _commandbufferBuilder.BuildAndExecute();
        }
// #if UNITY_EDITOR
//         else
//         {
//             // If we're not running, reset the frame data to avoid validation warnings
//             for (int i = 0; i < _lodTransform._renderData.Length; i++)
//             {
//                 _lodTransform._renderData[i]._frame = -1;
//             }
//             for (int i = 0; i < _lodTransform._renderDataSource.Length; i++)
//             {
//                 _lodTransform._renderDataSource[i]._frame = -1;
//             }
//         }
// #endif
    }

    void CreateDestroySubSystems()
    {
        {
            if (_lodDataHeightmap == null)
            {
                _lodDataHeightmap = new LODDataHeightmap(this);
                _lodDatas.Add(_lodDataHeightmap);
            }
        }
    }

        void WritePerFrameMaterialParams()
    {
        _lodTransform.WriteCascadeParams(_cascadeParamsTgt, _cascadeParamsSrc);
        _bufCascadeDataTgt.SetData(_cascadeParamsTgt);
        _bufCascadeDataSrc.SetData(_cascadeParamsSrc);

        WritePerCascadeInstanceData(_perCascadeInstanceData);
        _bufPerCascadeInstanceData.SetData(_perCascadeInstanceData);
    }

    // TODO: Look into fixed scale
    /// <summary>
    /// Could the terrain horizontal scale increase (for e.g. if the viewpoint gains altitude). Will be false if ocean already at maximum scale.
    /// </summary>
    public bool ScaleCouldIncrease { get { return _maxScale == -1f || Root.localScale.x < _maxScale * 0.99f; } }
    /// <summary>
    /// Could the terrain horizontal scale decrease (for e.g. if the viewpoint drops in altitude). Will be false if ocean already at minimum scale.
    /// </summary>
    public bool ScaleCouldDecrease { get { return _minScale == -1f || Root.localScale.x > _minScale * 1.01f; } }

    void WritePerCascadeInstanceData(PerCascadeInstanceData[] instanceData)
    {
        for (int lodIdx = 0; lodIdx < CurrentLodCount; lodIdx++)
        {
            // blend LOD 0 shape in/out to avoid pop, if the ocean might scale up later (it is smaller than its maximum scale)
            var needToBlendOutShape = lodIdx == 0 && ScaleCouldIncrease;
            instanceData[lodIdx]._meshScaleLerp = needToBlendOutShape ? ViewerAltitudeLevelAlpha : 0f;

            // blend furthest normals scale in/out to avoid pop, if scale could reduce
            var needToBlendOutNormals = lodIdx == CurrentLodCount - 1 && ScaleCouldDecrease;
            instanceData[lodIdx]._farNormalsWeight = needToBlendOutNormals ? ViewerAltitudeLevelAlpha : 1f;

            // geometry data
            // compute grid size of geometry. take the long way to get there - make sure we land exactly on a power of two
            // and not inherit any of the lossy-ness from lossyScale.
            var scale_pow_2 = CalcLodScale(lodIdx);
            instanceData[lodIdx]._geoGridWidth = scale_pow_2 / (0.25f * _lodDataResolution / _geometryDownSampleFactor);

            var mul = 1.875f; // fudge 1
            var pow = 1.4f; // fudge 2
            var texelWidth = instanceData[lodIdx]._geoGridWidth / _geometryDownSampleFactor;
        }
    }

    void LateUpdateLods()
    {
        // Do any per-frame update for each LOD type.
        _lodTransform.UpdateTransforms();

        _lodDataHeightmap?.UpdateLodData();
    }

    bool VerifyRequirements()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.LogError("Terrain does not support WebGL backends.", this);
            return false;
        }
#if UNITY_EDITOR
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
        {
            Debug.LogError("Terrain does not support OpenGL backends.", this);
            return false;
        }
#endif
        if (SystemInfo.graphicsShaderLevel < 45)
        {
            Debug.LogError("Terrain requires graphics devices that support shader level 4.5 or above.", this);
            return false;
        }
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("Terrain requires graphics devices that support compute shaders.", this);
            return false;
        }
        if (!SystemInfo.supports2DArrayTextures)
        {
            Debug.LogError("Terrain requires graphics devices that support 2D array textures.", this);
            return false;
        }

        return true;
    }

    void ValidateViewpoint()
    {
        if (Viewpoint == null)
        {
            Debug.LogError("Terrain renderer needs to know where to focus the ocean detail. Please set the <i>ViewCamera</i> or the <i>Viewpoint</i> property that will render the terrain, or tag the primary camera as <i>MainCamera</i>.", this);
        }
    }

    public bool Validate(TerrainRenderer terrain)
    {
        var isValid = true;

        if (_material == null)
        {
            Debug.Log
            (
                "A material for the terrain must be assigned on the Material property of the terrainRenderer."
            );

            isValid = false;
        }

        // terrainRenderer
        if (FindObjectsOfType<TerrainRenderer>().Length > 1)
        {
            Debug.Log
            (
                "Multiple terrainRenderer scripts detected in open scenes, this is not typical - usually only one terrainRenderer is expected to be present."
            );
        }

        // terrain Detail Parameters
        var baseMeshDensity = _lodDataResolution * 0.25f / _geometryDownSampleFactor;

        if (baseMeshDensity < 8)
        {
            Debug.Log
            (
                "Base mesh density is lower than 8. There will be visible gaps in the terrain surface. " +
                "Increase the <i>LOD Data Resolution</i> or decrease the <i>Geometry Down Sample Factor</i>."
            );
        }
        else if (baseMeshDensity < 16)
        {
            Debug.Log
            (
                "Base mesh density is lower than 16. There will be visible transitions when traversing the terrain surface. " +
                "Increase the <i>LOD Data Resolution</i> or decrease the <i>Geometry Down Sample Factor</i>."
            );
        }

        if (transform.eulerAngles.magnitude > 0.0001f)
        {
            Debug.Log
            (
                $"There must be no rotation on the terrain GameObject, and no rotation on any parent. Currently the rotation Euler angles are {transform.eulerAngles}."
            );
        }

        return isValid;
    }

    void LateUpdatePosition()
    {
        Vector3 pos = Viewpoint.position;

        // maintain y coordinate - sea level
        pos.y = Root.position.y;

        Root.position = pos;

        Shader.SetGlobalVector(sp_TerrainCenterPosWorld, Root.position);
    }

    void LateUpdateScale()
    {
        float camDistance = Mathf.Max(0.0f, Viewpoint.position.y - MaxVertDisplacement);

        // offset level of detail to keep max detail in a band near the surface
        camDistance = Mathf.Max(camDistance - 4f, 0f);

        // scale ocean mesh based on camera distance to sea level, to keep uniform detail.
        const float HEIGHT_LOD_MUL = 1f;
        float level = camDistance * HEIGHT_LOD_MUL;
        level = Mathf.Max(level, _minScale);
        if (_maxScale != -1f) level = Mathf.Min(level, 1.99f * _maxScale);

        float l2 = Mathf.Log(level) / Mathf.Log(2f);
        float l2f = Mathf.Floor(l2);

        ViewerAltitudeLevelAlpha = l2 - l2f;

        Scale = Mathf.Pow(2f, l2f);
        Root.localScale = new Vector3(Scale, 1f, Scale);
    }

    private void CleanUp()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying && Root != null)
        {
            DestroyImmediate(Root.gameObject);
        }
        else
#endif
        if (Root != null)
        {
            Destroy(Root.gameObject);
        }

        Root = null;

        _lodTransform = null;
        _lodDataHeightmap = null;

        _terrainChunkRenderers.Clear();

        _bufPerCascadeInstanceData.Dispose();
        _bufCascadeDataTgt.Dispose();
        _bufCascadeDataSrc.Dispose();
    }
}

/// <summary>
/// Instantiates all the ocean geometry, as a set of tiles.
/// </summary>
public static class TerrainBuilder
{
    // The comments below illustrate case when BASE_VERT_DENSITY = 2. The ocean mesh is built up from these patches. Rotational symmetry
    // is used where possible to eliminate combinations. The slim variants are used to eliminate overlap between patches.
    enum PatchType
    {
        /// <summary>
        /// Adds no skirt. Used in interior of highest detail LOD (0)
        ///
        ///    1 -------
        ///      |  |  |
        ///  z   -------
        ///      |  |  |
        ///    0 -------
        ///      0     1
        ///         x
        ///
        /// </summary>
        Interior,

        /// <summary>
        /// Adds a full skirt all of the way around a patch
        ///
        ///      -------------
        ///      |  |  |  |  |
        ///    1 -------------
        ///      |  |  |  |  |
        ///  z   -------------
        ///      |  |  |  |  |
        ///    0 -------------
        ///      |  |  |  |  |
        ///      -------------
        ///         0     1
        ///            x
        ///
        /// </summary>
        Fat,

        /// <summary>
        /// Adds a skirt on the right hand side of the patch
        ///
        ///    1 ----------
        ///      |  |  |  |
        ///  z   ----------
        ///      |  |  |  |
        ///    0 ----------
        ///      0     1
        ///         x
        ///
        /// </summary>
        FatX,

        /// <summary>
        /// Adds a skirt on the right hand side of the patch, removes skirt from top
        /// </summary>
        FatXSlimZ,

        /// <summary>
        /// Outer most side - this adds an extra skirt on the left hand side of the patch,
        /// which will point outwards and be extended to Zfar
        ///
        ///    1 --------------------------------------------------------------------------------------
        ///      |  |  |                                                                              |
        ///  z   --------------------------------------------------------------------------------------
        ///      |  |  |                                                                              |
        ///    0 --------------------------------------------------------------------------------------
        ///      0     1
        ///         x
        ///
        /// </summary>
        FatXOuter,

        /// <summary>
        /// Adds skirts at the top and right sides of the patch
        /// </summary>
        FatXZ,

        /// <summary>
        /// Adds skirts at the top and right sides of the patch and pushes them to horizon
        /// </summary>
        FatXZOuter,

        /// <summary>
        /// One less set of verts in x direction
        /// </summary>
        SlimX,

        /// <summary>
        /// One less set of verts in both x and z directions
        /// </summary>
        SlimXZ,

        /// <summary>
        /// One less set of verts in x direction, extra verts at start of z direction
        ///
        ///      ----
        ///      |  |
        ///    1 ----
        ///      |  |
        ///  z   ----
        ///      |  |
        ///    0 ----
        ///      0     1
        ///         x
        ///
        /// </summary>
        SlimXFatZ,

        /// <summary>
        /// Number of patch types
        /// </summary>
        Count,
    }

    public static Transform GenerateMesh(TerrainRenderer terrain, List<TerrainChunkRenderer> tiles, int lodDataResolution, int geoDownSampleFactor, int lodCount)
    {
        if (lodCount < 1)
        {
            Debug.LogError("Invalid LOD count: " + lodCount.ToString(), terrain);
            return null;
        }

        int oceanLayer = LayerMask.NameToLayer(terrain.LayerName);
        if (oceanLayer == -1)
        {
            Debug.LogError("Invalid ocean layer: " + terrain.LayerName + " please add this layer.", terrain);
            oceanLayer = 0;
        }

#if PROFILE_CONSTRUCTION
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

        // create mesh data
        Mesh[] meshInsts = new Mesh[(int)PatchType.Count];
        Bounds[] meshBounds = new Bounds[(int)PatchType.Count];
        // 4 tiles across a LOD, and support lowering density by a factor
        var tileResolution = Mathf.Round(0.25f * lodDataResolution / geoDownSampleFactor);
        for (int i = 0; i < (int)PatchType.Count; i++)
        {
            meshInsts[i] = BuildTerrainPatch((PatchType)i, tileResolution, out meshBounds[i]);
        }

        ClearOutTiles(terrain, tiles);

        var root = new GameObject("Root");
        root.hideFlags = terrain._hideTerrainTileGameObjects ? HideFlags.HideAndDontSave : HideFlags.DontSave;
        root.transform.parent = terrain.transform;
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        for (int i = 0; i < lodCount; i++)
        {
            CreateLOD(terrain, tiles, root.transform, i, lodCount, meshInsts, meshBounds, lodDataResolution, geoDownSampleFactor, oceanLayer);
        }

#if PROFILE_CONSTRUCTION
            sw.Stop();
            Debug.Log( "Finished generating " + lodCount.ToString() + " LODs, time: " + (1000.0*sw.Elapsed.TotalSeconds).ToString(".000") + "ms" );
#endif

        return root.transform;
    }

    public static void ClearOutTiles(TerrainRenderer terrain, List<TerrainChunkRenderer> tiles)
    {
        tiles.Clear();

        if (terrain.Root == null)
        {
            return;
        }

        // Remove existing LODs
        for (int i = 0; i < terrain.Root.childCount; i++)
        {
            var child = terrain.Root.GetChild(i);
            if (child.name.StartsWith("Tile_L"))
            {
                DestroyGO(child);

                i--;
            }
        }

        DestroyGO(terrain.Root);
    }

    static void DestroyGO(Transform go)
    {
        go.parent = null;

#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            Object.Destroy(go.gameObject);
        }
        else
        {
            Object.DestroyImmediate(go.gameObject);
        }
#else
            Object.Destroy(go.gameObject);
#endif
    }

    static void CreateLOD(TerrainRenderer terrain, List<TerrainChunkRenderer> tiles, Transform parent, int lodIndex, int lodCount, Mesh[] meshData, Bounds[] meshBounds, int lodDataResolution, int geoDownSampleFactor, int oceanLayer)
    {
        float horizScale = Mathf.Pow(2f, lodIndex);

        bool isBiggestLOD = lodIndex == lodCount - 1;
        bool generateSkirt = isBiggestLOD && !terrain._disableSkirt;

        Vector2[] offsets;
        PatchType[] patchTypes;

        PatchType leadSideType = generateSkirt ? PatchType.FatXOuter : PatchType.SlimX;
        PatchType trailSideType = generateSkirt ? PatchType.FatXOuter : PatchType.FatX;
        PatchType leadCornerType = generateSkirt ? PatchType.FatXZOuter : PatchType.SlimXZ;
        PatchType trailCornerType = generateSkirt ? PatchType.FatXZOuter : PatchType.FatXZ;
        PatchType tlCornerType = generateSkirt ? PatchType.FatXZOuter : PatchType.SlimXFatZ;
        PatchType brCornerType = generateSkirt ? PatchType.FatXZOuter : PatchType.FatXSlimZ;

        if (lodIndex != 0)
        {
            // instance indices:
            //    0  1  2  3
            //    4        5
            //    6        7
            //    8  9  10 11
            offsets = new Vector2[] {
                    new Vector2(-1.5f,1.5f),    new Vector2(-0.5f,1.5f),    new Vector2(0.5f,1.5f),     new Vector2(1.5f,1.5f),
                    new Vector2(-1.5f,0.5f),                                                            new Vector2(1.5f,0.5f),
                    new Vector2(-1.5f,-0.5f),                                                           new Vector2(1.5f,-0.5f),
                    new Vector2(-1.5f,-1.5f),   new Vector2(-0.5f,-1.5f),   new Vector2(0.5f,-1.5f),    new Vector2(1.5f,-1.5f),
                };

            // usually rings have an extra side of verts that point inwards. the outermost ring has both the inward
            // verts and also and additional outwards set of verts that go to the horizon
            patchTypes = new PatchType[] {
                    tlCornerType,         leadSideType,           leadSideType,         leadCornerType,
                    trailSideType,                                                      leadSideType,
                    trailSideType,                                                      leadSideType,
                    trailCornerType,      trailSideType,          trailSideType,        brCornerType,
                };
        }
        else
        {
            // first LOD has inside bit as well:
            //    0  1  2  3
            //    4  5  6  7
            //    8  9  10 11
            //    12 13 14 15
            offsets = new Vector2[] {
                    new Vector2(-1.5f,1.5f),    new Vector2(-0.5f,1.5f),    new Vector2(0.5f,1.5f),     new Vector2(1.5f,1.5f),
                    new Vector2(-1.5f,0.5f),    new Vector2(-0.5f,0.5f),    new Vector2(0.5f,0.5f),     new Vector2(1.5f,0.5f),
                    new Vector2(-1.5f,-0.5f),   new Vector2(-0.5f,-0.5f),   new Vector2(0.5f,-0.5f),    new Vector2(1.5f,-0.5f),
                    new Vector2(-1.5f,-1.5f),   new Vector2(-0.5f,-1.5f),   new Vector2(0.5f,-1.5f),    new Vector2(1.5f,-1.5f),
                };


            // all interior - the "side" types have an extra skirt that points inwards - this means that this inner most
            // section doesn't need any skirting. this is good - this is the highest density part of the mesh.
            patchTypes = new PatchType[] {
                    tlCornerType,       leadSideType,           leadSideType,           leadCornerType,
                    trailSideType,      PatchType.Interior,     PatchType.Interior,     leadSideType,
                    trailSideType,      PatchType.Interior,     PatchType.Interior,     leadSideType,
                    trailCornerType,    trailSideType,          trailSideType,          brCornerType,
                };
        }

        // debug toggle to force all patches to be the same. they'll be made with a surrounding skirt to make sure patches
        // overlap
        if (terrain._uniformTiles)
        {
            for (int i = 0; i < patchTypes.Length; i++)
            {
                patchTypes[i] = PatchType.Fat;
            }
        }

        // create the ocean patches
        for (int i = 0; i < offsets.Length; i++)
        {
            // instantiate and place patch
            var patch = new GameObject($"Tile_L{lodIndex}_{patchTypes[i]}");
            patch.hideFlags = HideFlags.DontSave;
            patch.layer = oceanLayer;
            patch.transform.parent = parent;
            Vector2 pos = offsets[i];
            patch.transform.localPosition = horizScale * new Vector3(pos.x, 0f, pos.y);
            // scale only horizontally, otherwise culling bounding box will be scaled up in y
            patch.transform.localScale = new Vector3(horizScale, 1f, horizScale);

            {
                var terrainChunkRenderer = patch.AddComponent<TerrainChunkRenderer>();
                terrainChunkRenderer._boundsLocal = meshBounds[(int)patchTypes[i]];
                patch.AddComponent<MeshFilter>().sharedMesh = meshData[(int)patchTypes[i]];
                terrainChunkRenderer.SetInstanceData(lodIndex, lodCount, lodDataResolution, geoDownSampleFactor);
                tiles.Add(terrainChunkRenderer);
            }

            var mr = patch.AddComponent<MeshRenderer>();

            // Sorting order to stop unity drawing it back to front. make the innermost 4 tiles draw first, followed by
            // the rest of the tiles by LOD index. all this happens before layer 0 - the sorting layer takes priority over the
            // render queue it seems! ( https://cdry.wordpress.com/2017/04/28/unity-render-queues-vs-sorting-layers/ ). This pushes
            // terrain rendering way early, so transparent objects will by default render afterwards
            mr.sortingOrder = -lodCount + (patchTypes[i] == PatchType.Interior ? -1 : lodIndex);

            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; // arbitrary - could be turned on if desired
            mr.receiveShadows = true; // this setting is ignored by unity for the transparent ocean shader
            mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            mr.material = terrain.TerrainMaterial;

            // rotate side patches to point the +x side outwards
            bool rotateXOutwards = patchTypes[i] == PatchType.FatX || patchTypes[i] == PatchType.FatXOuter || patchTypes[i] == PatchType.SlimX || patchTypes[i] == PatchType.SlimXFatZ;
            if (rotateXOutwards)
            {
                if (Mathf.Abs(pos.y) >= Mathf.Abs(pos.x))
                    patch.transform.localEulerAngles = -Vector3.up * 90f * Mathf.Sign(pos.y);
                else
                    patch.transform.localEulerAngles = pos.x < 0f ? Vector3.up * 180f : Vector3.zero;
            }

            // rotate the corner patches so the +x and +z sides point outwards
            bool rotateXZOutwards = patchTypes[i] == PatchType.FatXZ || patchTypes[i] == PatchType.SlimXZ || patchTypes[i] == PatchType.FatXSlimZ || patchTypes[i] == PatchType.FatXZOuter;
            if (rotateXZOutwards)
            {
                // xz direction before rotation
                Vector3 from = new Vector3(1f, 0f, 1f).normalized;
                // target xz direction is outwards vector given by local patch position - assumes this patch is a corner (checked below)
                Vector3 to = patch.transform.localPosition.normalized;
                if (Mathf.Abs(patch.transform.localPosition.x) < 0.0001f || Mathf.Abs(Mathf.Abs(patch.transform.localPosition.x) - Mathf.Abs(patch.transform.localPosition.z)) > 0.001f)
                {
                    Debug.LogWarning("Skipped rotating a patch because it isn't a corner, click here to highlight.", patch);
                    continue;
                }

                // Detect 180 degree rotations as it doesn't always rotate around Y
                if (Vector3.Dot(from, to) < -0.99f)
                    patch.transform.localEulerAngles = Vector3.up * 180f;
                else
                    patch.transform.localRotation = Quaternion.FromToRotation(from, to);
            }
        }
    }

    static Mesh BuildTerrainPatch(PatchType pt, float vertDensity, out Bounds bounds)
    {
        ArrayList verts = new ArrayList();
        ArrayList indices = new ArrayList();

        // stick a bunch of verts into a 1m x 1m patch (scaling happens later)
        float dx = 1f / vertDensity;

        //////////////////////////////////////////////////////////////////////////////////
        // verts

        // see comments within PatchType for diagrams of each patch mesh

        // skirt widths on left, right, bottom and top (in order)
        float skirtXminus = 0f, skirtXplus = 0f;
        float skirtZminus = 0f, skirtZplus = 0f;
        // set the patch size
        if (pt == PatchType.Fat) { skirtXminus = skirtXplus = skirtZminus = skirtZplus = 1f; }
        else if (pt == PatchType.FatX || pt == PatchType.FatXOuter) { skirtXplus = 1f; }
        else if (pt == PatchType.FatXZ || pt == PatchType.FatXZOuter) { skirtXplus = skirtZplus = 1f; }
        else if (pt == PatchType.FatXSlimZ) { skirtXplus = 1f; skirtZplus = -1f; }
        else if (pt == PatchType.SlimX) { skirtXplus = -1f; }
        else if (pt == PatchType.SlimXZ) { skirtXplus = skirtZplus = -1f; }
        else if (pt == PatchType.SlimXFatZ) { skirtXplus = -1f; skirtZplus = 1f; }

        float sideLength_verts_x = 1f + vertDensity + skirtXminus + skirtXplus;
        float sideLength_verts_z = 1f + vertDensity + skirtZminus + skirtZplus;

        float start_x = -0.5f - skirtXminus * dx;
        float start_z = -0.5f - skirtZminus * dx;
        float end_x = 0.5f + skirtXplus * dx;
        float end_z = 0.5f + skirtZplus * dx;

        for (float j = 0; j < sideLength_verts_z; j++)
        {
            // interpolate z across patch
            float z = Mathf.Lerp(start_z, end_z, j / (sideLength_verts_z - 1f));

            // push outermost edge out to horizon
            if (pt == PatchType.FatXZOuter && j == sideLength_verts_z - 1f)
                z *= 100f;

            for (float i = 0; i < sideLength_verts_x; i++)
            {
                // interpolate x across patch
                float x = Mathf.Lerp(start_x, end_x, i / (sideLength_verts_x - 1f));

                // push outermost edge out to horizon
                if (i == sideLength_verts_x - 1f && (pt == PatchType.FatXOuter || pt == PatchType.FatXZOuter))
                    x *= 100f;

                // could store something in y, although keep in mind this is a shared mesh that is shared across multiple lods
                verts.Add(new Vector3(x, 0f, z));
            }
        }


        //////////////////////////////////////////////////////////////////////////////////
        // indices

        int sideLength_squares_x = (int)sideLength_verts_x - 1;
        int sideLength_squares_z = (int)sideLength_verts_z - 1;

        for (int j = 0; j < sideLength_squares_z; j++)
        {
            for (int i = 0; i < sideLength_squares_x; i++)
            {
                bool flipEdge = false;

                if (i % 2 == 1) flipEdge = !flipEdge;
                if (j % 2 == 1) flipEdge = !flipEdge;

                int i0 = i + j * (sideLength_squares_x + 1);
                int i1 = i0 + 1;
                int i2 = i0 + (sideLength_squares_x + 1);
                int i3 = i2 + 1;

                if (!flipEdge)
                {
                    // tri 1
                    indices.Add(i3);
                    indices.Add(i1);
                    indices.Add(i0);

                    // tri 2
                    indices.Add(i0);
                    indices.Add(i2);
                    indices.Add(i3);
                }
                else
                {
                    // tri 1
                    indices.Add(i3);
                    indices.Add(i1);
                    indices.Add(i2);

                    // tri 2
                    indices.Add(i0);
                    indices.Add(i2);
                    indices.Add(i1);
                }
            }
        }


        //////////////////////////////////////////////////////////////////////////////////
        // create mesh

        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.DontSave;
        if (verts != null && verts.Count > 0)
        {
            Vector3[] arrV = new Vector3[verts.Count];
            verts.CopyTo(arrV);

            int[] arrI = new int[indices.Count];
            indices.CopyTo(arrI);

            mesh.SetIndices(null, MeshTopology.Triangles, 0);
            mesh.vertices = arrV;
            mesh.normals = null;
            mesh.SetIndices(arrI, MeshTopology.Triangles, 0);

            // recalculate bounds. add a little allowance for snapping. in the chunk renderer script, the bounds will be expanded further
            // to allow for horizontal displacement
            mesh.RecalculateBounds();
            bounds = mesh.bounds;
            bounds.extents = new Vector3(bounds.extents.x + dx, bounds.extents.y, bounds.extents.z + dx);
            mesh.bounds = bounds;
            mesh.name = pt.ToString();
        }
        else
        {
            bounds = new Bounds();
        }

        return mesh;
    }
}

#if UNITY_EDITOR


[CustomEditor(typeof(TerrainRenderer))]
public class OceanRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var target = this.target as TerrainRenderer;

        if (GUILayout.Button("Rebuild Terrain"))
        {
            target.enabled = false;
            target.enabled = true;
        }
    }
}

/// <summary>
/// Provides general helper functions for the editor.
/// </summary>
public static class EditorHelpers
{
    static EditorWindow _lastGameOrSceneEditorWindow = null;

    /// <summary>
    /// Returns the scene view camera if the scene view is focused.
    /// </summary>
    public static Camera GetActiveSceneViewCamera()
    {
        Camera sceneCamera = null;

        if (EditorWindow.focusedWindow != null && (EditorWindow.focusedWindow.titleContent.text == "Scene" ||
            EditorWindow.focusedWindow.titleContent.text == "Game"))
        {
            _lastGameOrSceneEditorWindow = EditorWindow.focusedWindow;
        }

        // If scene view is focused, use its camera. This code is slightly ropey but seems to work ok enough.
        if (_lastGameOrSceneEditorWindow != null && _lastGameOrSceneEditorWindow.titleContent.text == "Scene")
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && !EditorApplication.isPlaying)
            {
                sceneCamera = sceneView.camera;
            }
        }

        return sceneCamera;
    }
}

#endif
