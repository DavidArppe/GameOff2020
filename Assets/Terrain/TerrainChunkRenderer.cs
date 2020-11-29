using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class TerrainChunkRenderer : MonoBehaviour
{
    public bool _drawRenderBounds = false;

    public Bounds _boundsLocal;
    Mesh _mesh;
    public Renderer Rend { get; private set; }
    PropertyWrapperMPB _mpb;

    // Cache these off to support regenerating terrain surface
    int _lodIndex = -1;
    int _totalLodCount = -1;
    int _lodDataResolution = 256;
    int _geoDownSampleFactor = 1;

    static Camera _currentCamera = null;

    void Start()
    {
        Rend = GetComponent<Renderer>();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            _mesh = GetComponent<MeshFilter>().sharedMesh;
        }
        else
#endif
        {
            // An unshared mesh will break instancing, but a shared mesh will break culling due to shared bounds.
            _mesh = GetComponent<MeshFilter>().mesh;
        }

        UpdateMeshBounds();

        SetOneTimeMPBParams();
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= EditorUpdate;
        UnityEditor.EditorApplication.update += EditorUpdate;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= EditorUpdate;
#endif
    }

    void UpdateMeshBounds()
    {
#if UNITY_EDITOR
        if (this == null || transform == null || _mesh == null) return;
#endif
        var newBounds = _boundsLocal;
        ExpandBoundsForDisplacements(transform, ref newBounds);
        _mesh.bounds = newBounds;
    }

    private static void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        _currentCamera = camera;
    }

    void SetOneTimeMPBParams()
    {
        if (_mpb == null)
        {
            _mpb = new PropertyWrapperMPB();
        }

        Rend.GetPropertyBlock(_mpb.materialPropertyBlock);

        _mpb.SetInt(LODDataManager.sp_LD_SliceIndex, _lodIndex);

        Rend.SetPropertyBlock(_mpb.materialPropertyBlock);
    }

    // Called when visible to a camera
    void AlwaysUpdate()
    {
        if (TerrainRenderer.Instance == null || Rend == null)
        {
            return;
        }

        // check if built-in pipeline being used
        if (Camera.current != null)
        {
            _currentCamera = Camera.current;
            _currentCamera.depthTextureMode |= DepthTextureMode.Depth;
        }


        if (Rend.sharedMaterial != TerrainRenderer.Instance.TerrainMaterial)
        {
            Rend.sharedMaterial = TerrainRenderer.Instance.TerrainMaterial;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false) return;
#endif

        // This needs to be called on Update because the bounds depend on transform scale which can change. Also OnWillRenderObject depends on
        // the bounds being correct. This could however be called on scale change events, but would add slightly more complexity.
        UpdateMeshBounds();

        var distance = Vector2.SqrMagnitude(new Vector2(transform.position.x, transform.position.z));
        var checkDist = 8192.0f + Rend.bounds.extents.magnitude;

        if (distance > checkDist * checkDist)
        {
            Rend.enabled = false;
        }
        else
        {
            Rend.enabled = true;
        }

        AlwaysUpdate();
    }

#if UNITY_EDITOR
    private void EditorUpdate()
    {
        UpdateMeshBounds();

        var distance = Vector2.SqrMagnitude(new Vector2(transform.position.x, transform.position.z));
        var checkDist = 8192.0f + Rend.bounds.extents.magnitude;

        if (Rend.bounds.max.z < -8192.0f || Rend.bounds.min.z > 8192.0f ||
            Rend.bounds.max.x < -8192.0f || Rend.bounds.min.x > 8192.0f)
        {
            Rend.enabled = false;
        }
        else
        {
            Rend.enabled = true;
        }

        AlwaysUpdate();
    }
#endif

    // this is called every frame because the bounds are given in world space and depend on the transform scale, which
    // can change depending on view altitude
    public static void ExpandBoundsForDisplacements(Transform transform, ref Bounds bounds)
    {
        var boundsY = TerrainRenderer.Instance.MaxVertDisplacement;
        // extend the kinematic bounds slightly to give room for dynamic sim stuff
        boundsY += 5f;
        bounds.extents = new Vector3(bounds.extents.x, boundsY / transform.lossyScale.y, bounds.extents.z);
    }

    public void SetInstanceData(int lodIndex, int totalLodCount, int lodDataResolution, int geoDownSampleFactor)
    {
        _lodIndex = lodIndex; _totalLodCount = totalLodCount; _lodDataResolution = lodDataResolution; _geoDownSampleFactor = geoDownSampleFactor;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitStatics()
    {
        _currentCamera = null;
    }

    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
    }

    private void OnDrawGizmos()
    {
        if (_drawRenderBounds)
        {
            Rend.bounds.GizmosDraw();
        }
    }
}

public static class BoundsHelper
{
    public static void DebugDraw(this Bounds b)
    {
        var xmin = b.min.x;
        var ymin = b.min.y;
        var zmin = b.min.z;
        var xmax = b.max.x;
        var ymax = b.max.y;
        var zmax = b.max.z;

        Debug.DrawLine(new Vector3(xmin, ymin, zmin), new Vector3(xmin, ymin, zmax));
        Debug.DrawLine(new Vector3(xmin, ymin, zmin), new Vector3(xmax, ymin, zmin));
        Debug.DrawLine(new Vector3(xmax, ymin, zmax), new Vector3(xmin, ymin, zmax));
        Debug.DrawLine(new Vector3(xmax, ymin, zmax), new Vector3(xmax, ymin, zmin));

        Debug.DrawLine(new Vector3(xmin, ymax, zmin), new Vector3(xmin, ymax, zmax));
        Debug.DrawLine(new Vector3(xmin, ymax, zmin), new Vector3(xmax, ymax, zmin));
        Debug.DrawLine(new Vector3(xmax, ymax, zmax), new Vector3(xmin, ymax, zmax));
        Debug.DrawLine(new Vector3(xmax, ymax, zmax), new Vector3(xmax, ymax, zmin));

        Debug.DrawLine(new Vector3(xmax, ymax, zmax), new Vector3(xmax, ymin, zmax));
        Debug.DrawLine(new Vector3(xmin, ymin, zmin), new Vector3(xmin, ymax, zmin));
        Debug.DrawLine(new Vector3(xmax, ymin, zmin), new Vector3(xmax, ymax, zmin));
        Debug.DrawLine(new Vector3(xmin, ymax, zmax), new Vector3(xmin, ymin, zmax));
    }

    public static void GizmosDraw(this Bounds b)
    {
        var xmin = b.min.x;
        var ymin = b.min.y;
        var zmin = b.min.z;
        var xmax = b.max.x;
        var ymax = b.max.y;
        var zmax = b.max.z;

        Gizmos.DrawLine(new Vector3(xmin, ymin, zmin), new Vector3(xmin, ymin, zmax));
        Gizmos.DrawLine(new Vector3(xmin, ymin, zmin), new Vector3(xmax, ymin, zmin));
        Gizmos.DrawLine(new Vector3(xmax, ymin, zmax), new Vector3(xmin, ymin, zmax));
        Gizmos.DrawLine(new Vector3(xmax, ymin, zmax), new Vector3(xmax, ymin, zmin));

        Gizmos.DrawLine(new Vector3(xmin, ymax, zmin), new Vector3(xmin, ymax, zmax));
        Gizmos.DrawLine(new Vector3(xmin, ymax, zmin), new Vector3(xmax, ymax, zmin));
        Gizmos.DrawLine(new Vector3(xmax, ymax, zmax), new Vector3(xmin, ymax, zmax));
        Gizmos.DrawLine(new Vector3(xmax, ymax, zmax), new Vector3(xmax, ymax, zmin));

        Gizmos.DrawLine(new Vector3(xmax, ymax, zmax), new Vector3(xmax, ymin, zmax));
        Gizmos.DrawLine(new Vector3(xmin, ymin, zmin), new Vector3(xmin, ymax, zmin));
        Gizmos.DrawLine(new Vector3(xmax, ymin, zmin), new Vector3(xmax, ymax, zmin));
        Gizmos.DrawLine(new Vector3(xmin, ymax, zmax), new Vector3(xmin, ymin, zmax));
    }
}

public interface IFloatingOrigin
{
    /// <summary>
    /// Set a new origin. This is equivalent to subtracting the new origin position from any world position state.
    /// </summary>
    void SetOrigin(Vector3 newOrigin);
}
