// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Base class for the command buffer builder, which takes care of updating all terrain-related data. If you wish to provide your
/// own update logic, you can create a new component that inherits from this class and attach it to the same GameObject as the
/// TerrainRenderer script. The new component should be set to update after the Default bucket, similar to BuildCommandBuffer.
/// </summary>
public abstract class BuildCommandBufferBase
{
    /// <summary>
    /// Used to validate update order
    /// </summary>
    public static int _lastUpdateFrame = -1;

#if UNITY_2019_3_OR_NEWER
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
    static void InitStatics()
    {
        // Init here from 2019.3 onwards
        _lastUpdateFrame = -1;
    }
}

public class BuildCommandBuffer : BuildCommandBufferBase
{
    CommandBuffer _buf;

    void BuildLodData(TerrainRenderer terrain, CommandBuffer buf)
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // --- Heightmap
        if (terrain != null && terrain._lodDataHeightmap.enabled)
        {
            terrain._lodDataHeightmap.BuildCommandBuffer(terrain, buf);
        }
    }

    /// <summary>
    /// Construct the command buffer and attach it to the camera so that it will be executed in the render.
    /// </summary>
    public void BuildAndExecute()
    {
        if (TerrainRenderer.Instance == null) return;

        if (_buf == null)
        {
            _buf = new CommandBuffer();
            _buf.name = "TerrainLODData";
        }

        _buf.Clear();

        BuildLodData(TerrainRenderer.Instance, _buf);

        // This will execute at the beginning of the frame before the graphics queue
        Graphics.ExecuteCommandBuffer(_buf);

        _lastUpdateFrame = TerrainRenderer.FrameCount;
    }
}
