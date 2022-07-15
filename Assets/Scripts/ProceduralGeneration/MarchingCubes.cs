using cosmicpotato.noisetools.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

struct Triangle
{
    public Vector3 A, B, C;
}

[ExecuteInEditMode]
public class MarchingCubes : MonoBehaviour
{
    public ComputeShader mcShader;  // marching cubes shader
    public Material material;       // material of map

    public Vector3 scale = new Vector3(1, 1, 1);        // scale of map
    public Vector3 mapSize = new Vector3(1, 1, 1);      // map size in chunks
    public int maxChunkSize = 10000;                    // max chunk area
    public Vector3 chunkSize = new Vector3(16, 16, 16); // chunk size
    [Range(0, 1)] public float threshold = 0.5f;        // noise threshold
    public bool realtimeGeneration = false;             // update mesh as values are changed in the inspector

    [HideInInspector] public Noise3D noise;             // noise function 

    int mcShaderID;                 // id of shader
    RenderTexture heightmapBuffer;  // heightmap render texture passed to shader
    ComputeBuffer triangleBuffer;   // triangle vertex buffer passed to shader
    Triangle[] triangles;           // triangle array
    ComputeBuffer triCountBuffer;   // triangle count buffer passed to shader
    int numTris;                    // number of triangles

    /// <summary>
    /// Initialize triangle and triangle count buffers
    /// </summary>
    public void InitBuffers()
    {
        // setup local arrays
        int maxTris = (int)(chunkSize.x * chunkSize.y * chunkSize.z);
        if (maxTris > maxChunkSize)
        {
            Debug.LogWarning("chunk size max reached!");
            maxTris = maxChunkSize;
        }
        triangles = new Triangle[maxTris * 5];

        // setup GPU buffers
        triangleBuffer = new ComputeBuffer(triangles.Length, sizeof(float) * 9, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }

    /// <summary>
    /// Release all buffers and textures
    /// </summary>
    public void ReleaseBuffers()
    {
        triCountBuffer.Release();
        triangleBuffer.Release();
        RenderTexture.active = null;
        heightmapBuffer.Release();
    }

    /// <summary>
    /// Set up shaders and link buffers
    /// </summary>
    public void InitShader()
    {
        // setup shader
        mcShaderID = mcShader.FindKernel("MeshGen");
        mcShader.SetTexture(mcShaderID, "noiseTexture", heightmapBuffer);
        triangleBuffer.SetCounterValue(0);
        mcShader.SetBuffer(mcShaderID, "triangles", triangleBuffer);
        mcShader.SetFloats("scale", new float[] { scale.x, scale.y, scale.z });
        mcShader.SetInt("xdim", (int)chunkSize.x);
        mcShader.SetInt("ydim", (int)chunkSize.y);
        mcShader.SetInt("zdim", (int)chunkSize.z);
        mcShader.SetFloat("isoLevel", threshold);
    }

    /// <summary>
    /// Dispatch shader and read buffers
    /// </summary>
    public void DispatchShader()
    {
        // find threadgroup sizes
        uint kx = 0, ky = 0, kz = 0;
        mcShader.GetKernelThreadGroupSizes(mcShaderID, out kx, out ky, out kz);
        mcShader.Dispatch(mcShaderID, (int)((float)chunkSize.x / (float)kx) + 1, (int)((float)chunkSize.y / (float)ky) + 1, (int)((float)chunkSize.z / (float)kz) + 1);

        // get data from GPU
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        numTris = triCountArray[0];
        triangleBuffer.GetData(triangles);
    }

    /// <summary>
    /// Generate 3D mesh chunks
    /// </summary>
    public void GenerateMesh()
    {
        InitBuffers();

        // get list of children
        List<Transform> children = new List<Transform>(gameObject.GetComponentsInChildren<Transform>(false));
        if (children.Contains(this.transform))
            children.Remove(this.transform);

        // iterate over xyz chunks
        int chunkIndex = 0;
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    chunkIndex = x + (int)mapSize.x * y + (int)mapSize.x * (int)mapSize.y * z;
                    string name = string.Format("chunk ({0}, {1}, {2})", x, y, z);

                    // make chunk if it doesn't exist
                    GameObject g;
                    if (chunkIndex >= children.Count)
                    {
                        g = new GameObject(name);
                        g.transform.SetParent(transform);
                        g.AddComponent<MeshFilter>();
                        g.AddComponent<MeshRenderer>();
                        children.Add(g.transform);
                    }
                    else
                    {
                        g = children[chunkIndex].gameObject;
                        g.name = name;
                    }

                    // set transform
                    g.transform.rotation = transform.rotation;
                    g.transform.position = transform.rotation * Vector3.Scale(Vector3.Scale(new Vector3(x * chunkSize.x, y * chunkSize.y, z * chunkSize.z), scale), transform.localScale);
                    g.transform.position += transform.position;

                    // get heightmap
                    int res = (int)Mathf.Max(chunkSize.x, Mathf.Max(chunkSize.y, chunkSize.z)) + 1;
                    Vector3 offset = new Vector3(chunkSize.x / (noise.scale.x * res), chunkSize.y / (noise.scale.y * res), chunkSize.z / (noise.scale.z * res));
                    heightmapBuffer = noise.CalculateNoise(noise.offset + Vector3.Scale(new Vector3(x, y, z), offset), noise.scale, res);
                    InitShader();
                    DispatchShader();

                    // get vertices and indices from triangles
                    Vector3[] vertices = new Vector3[numTris * 3];
                    int[] indices = new int[numTris * 3];
                    for (int i = 0; i < numTris; i++)
                    {
                        vertices[i * 3] = triangles[i].A;
                        vertices[i * 3 + 1] = triangles[i].B;
                        vertices[i * 3 + 2] = triangles[i].C;
                        indices[i * 3] = i * 3;
                        indices[i * 3 + 1] = i * 3 + 1;
                        indices[i * 3 + 2] = i * 3 + 2;
                    }

                    // set mesh
                    Mesh mesh = new Mesh();
                    mesh.vertices = vertices;
                    mesh.triangles = indices;
                    mesh.Optimize();
                    mesh.RecalculateNormals();
                    g.GetComponent<MeshFilter>().mesh = mesh;
                    g.GetComponent<Renderer>().material = material;
                }
            }
        }
        ReleaseBuffers();

        // destroy all unused chunks
        for (int i = children.Count; i > chunkIndex + 1; i--)
        {
            DestroyImmediate(children[chunkIndex + 1].gameObject);
            children.RemoveAt(chunkIndex + 1);
        }
    }

    /// <summary>
    /// Destroy all chunks
    /// </summary>
    public void ClearMesh()
    {
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);
    }
}
