using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using TerrainInput = EfficientSortedList<int, ILodDataInput>;

/// <summary>
/// Comparer that always returns less or greater, never equal, to get work around unique key constraint
/// </summary>
public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        // If non-zero, use result, otherwise return greater (never equal)
        return result != 0 ? result : 1;
    }
}

public interface ILodDataInput
{
    void Draw(CommandBuffer buf, float weight, int isTransition, int lodIdx);
    float Wavelength { get; }
    bool Enabled { get; }
}

/// <summary>
/// Base class for scripts that register input to the various LOD data types.
/// </summary>
[ExecuteAlways]
public abstract partial class RegisterLodDataInputBase : MonoBehaviour, ILodDataInput
{
#if UNITY_EDITOR
    [SerializeField, Tooltip("Check that the shader applied to this object matches the input type (so e.g. an Animated Waves input object has an Animated Waves input shader.")]
    bool _checkShaderName = true;
#endif

    public abstract float Wavelength { get; }

    public abstract bool Enabled { get; }

    public static int sp_Weight = Shader.PropertyToID("_Weight");
    public static int sp_DisplacementAtInputPosition = Shader.PropertyToID("_DisplacementAtInputPosition");

    protected virtual bool FollowHorizontalMotion => false;

    protected abstract string ShaderPrefix { get; }

    static DuplicateKeyComparer<int> s_comparer = new DuplicateKeyComparer<int>();
    static Dictionary<Type, TerrainInput> s_registrar = new Dictionary<Type, TerrainInput>();

    public static TerrainInput GetRegistrar(Type lodDataMgrType)
    {
        TerrainInput registered;
        if (!s_registrar.TryGetValue(lodDataMgrType, out registered))
        {
            registered = new TerrainInput(s_comparer);
            s_registrar.Add(lodDataMgrType, registered);
        }
        return registered;
    }

    protected Renderer _renderer;
    protected Material _material;

    void InitRendererAndMaterial(bool verifyShader)
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer)
        {
#if UNITY_EDITOR
            if (Application.isPlaying && _checkShaderName && verifyShader)
            {
                var renderer = gameObject.GetComponent<Renderer>();
                if (!renderer.sharedMaterial || renderer.sharedMaterial.shader && !renderer.sharedMaterial.shader.name.StartsWith(ShaderPrefix))
                {
                    Debug.LogError
                    (
                        $"Shader assigned to terrain input expected to be of type <i>{ShaderPrefix}</i>."
                    );
                }
            }
#endif

            _material = _renderer.sharedMaterial;
        }
    }

    protected void Start()
    {
        InitRendererAndMaterial(true);
    }

    protected virtual void Update()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            InitRendererAndMaterial(true);
        }
#endif
    }

    public void Draw(CommandBuffer buf, float weight, int isTransition, int lodIdx)
    {
        if (_renderer && _material && weight > 0f)
        {
            buf.SetGlobalFloat(sp_Weight, weight);
            buf.SetGlobalFloat(LODDataManager.sp_LD_SliceIndex, lodIdx);

            _material.SetVector(sp_DisplacementAtInputPosition, Vector3.zero);

            buf.DrawRenderer(_renderer, _material);
        }
    }

#if UNITY_2019_3_OR_NEWER
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
    static void InitStatics()
    {
        // Init here from 2019.3 onwards
        s_registrar.Clear();
        sp_Weight = Shader.PropertyToID("_Weight");
    }
}

/// <summary>
/// Registers input to a particular LOD data.
/// </summary>
[ExecuteAlways]
public abstract class RegisterLodDataInput<LodDataType> : RegisterLodDataInputBase
    where LodDataType : LODDataManager
{
    [SerializeField] bool _disableRenderer = true;

    protected abstract Color GizmoColor { get; }

    int _registeredQueueValue = int.MinValue;

    bool GetQueue(out int queue)
    {
        var rend = GetComponent<Renderer>();
        if (rend && rend.sharedMaterial != null)
        {
            queue = rend.sharedMaterial.renderQueue;
            return true;
        }
        queue = int.MinValue;
        return false;
    }

    protected virtual void OnEnable()
    {
        if (_disableRenderer)
        {
            var rend = GetComponent<Renderer>();
            if (rend)
            {
                rend.enabled = false;
            }
        }

        int q;
        GetQueue(out q);

        var registrar = GetRegistrar(typeof(LodDataType));
        registrar.Add(q, this);
        _registeredQueueValue = q;
    }

    protected virtual void OnDisable()
    {
        var registrar = GetRegistrar(typeof(LodDataType));
        if (registrar != null)
        {
            registrar.Remove(this);
        }
    }

    protected override void Update()
    {
        base.Update();

#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            int q;
            if (GetQueue(out q))
            {
                if (q != _registeredQueueValue)
                {
                    var registrar = GetRegistrar(typeof(LodDataType));
                    registrar.Remove(this);
                    registrar.Add(q, this);
                    _registeredQueueValue = q;
                }
            }
        }
#endif
    }

    private void OnDrawGizmosSelected()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf)
        {
            Gizmos.color = GizmoColor;
            Gizmos.DrawWireMesh(mf.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
        }
    }
}

[ExecuteAlways]
public abstract class RegisterLodDataInputDisplacementCorrection<LodDataType> : RegisterLodDataInput<LodDataType>
    where LodDataType : LODDataManager
{
    [SerializeField, Tooltip("Whether this input data should displace horizontally with waves. If false, data will not move from side to side with the waves. Adds a small performance overhead when disabled.")]
    bool _followHorizontalMotion = false;

    protected override bool FollowHorizontalMotion => _followHorizontalMotion;
}

#if UNITY_EDITOR
public abstract partial class RegisterLodDataInputBase
{

}

#endif
