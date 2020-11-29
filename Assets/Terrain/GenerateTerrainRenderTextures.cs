using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GenerateTerrainRenderTextures : MonoBehaviour
{
    public int textureDim = 8192;
    public float sampleScale = 1.0f;

    public static GenerateTerrainRenderTextures Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<GenerateTerrainRenderTextures>();
            return _instance;
        }
    }
    private static GenerateTerrainRenderTextures _instance;


    public void GenerateTexture()
    {
        float halfDim = (float)textureDim / 2.0f;

        Texture2D heightTexture = new Texture2D(textureDim, textureDim, TextureFormat.RGBAFloat, false, true);
        float[] colors = new float[textureDim * textureDim * 4];
        byte[] byteArray = new byte[colors.Length * sizeof(float)];


        int x = 0; for (float xPos = -halfDim + 0.5f; x < textureDim; xPos += 1.0f, x++)
        {
            int y = 0; for (float yPos = -halfDim + 0.5f; y < textureDim; yPos += 1.0f, y++)
            {
                var samplePos = new Vector3(xPos * sampleScale, 0.0f, yPos * sampleScale);

                var hits = Physics.RaycastAll(samplePos + Vector3.up * 2000.0f, Vector3.down, 4000.0f);
                colors[(y * textureDim + x) * 4 + 0] = 0.0f;
                colors[(y * textureDim + x) * 4 + 1] = 0.0f;
                colors[(y * textureDim + x) * 4 + 2] = 0.0f;
                colors[(y * textureDim + x) * 4 + 3] = 0.0f;

                foreach (var hit in hits)
                {
                    var terrain = hit.collider.gameObject.GetComponent<Terrain>();

                    if (terrain != null)
                    {
                        var terrainLocalPos = samplePos - terrain.transform.position;
                        var normalizedPos = new Vector2(Mathf.InverseLerp(0.0f, terrain.terrainData.size.x, terrainLocalPos.x),
                                                    Mathf.InverseLerp(0.0f, terrain.terrainData.size.z, terrainLocalPos.z));
                        var terrainNormal = terrain.terrainData.GetInterpolatedNormal(normalizedPos.x, normalizedPos.y);

                        colors[(y * textureDim + x) * 4 + 0] = terrain.SampleHeight(samplePos) / terrain.terrainData.size.y;
                        colors[(y * textureDim + x) * 4 + 1] = terrainNormal.x * 0.5f + 0.5f;
                        colors[(y * textureDim + x) * 4 + 2] = terrainNormal.y * 0.5f + 0.5f;
                        colors[(y * textureDim + x) * 4 + 3] = terrainNormal.z * 0.5f + 0.5f;
                        break;
                    }
                }
            }
        }


        Buffer.BlockCopy(colors, 0, byteArray, 0, byteArray.Length);
        heightTexture.LoadRawTextureData(byteArray);
        heightTexture.Apply();

        File.WriteAllBytes(Path.Combine(
            Application.dataPath,
            string.Format("Artwork/Resources/MoonTextures/{0}-Heightmap.exr", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Replace(' ', '_'))),
            heightTexture.EncodeToEXR());

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(textureDim * sampleScale, 1000.0f, textureDim * sampleScale));
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(GenerateTerrainRenderTextures)), CanEditMultipleObjects]
public class GenerateTerrainRenderTexturesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var targetPlayer = (GenerateTerrainRenderTextures)target;

        DrawDefaultInspector();

        var filePath = string.Format("Artwork/Resources/MoonTextures/{0}-Heightmap.exr", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Replace(' ', '_'));
        EditorGUILayout.HelpBox(string.Format("This operation will save the textures to {0}", filePath), MessageType.Info);
        if (GUILayout.Button("Generate Textures"))
        {
            targetPlayer.GenerateTexture();
        }
    }
}

#endif
