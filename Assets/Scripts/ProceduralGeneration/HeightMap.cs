using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class HeightMap : MonoBehaviour
{
    public ComputeShader hmShader;  // heightmap compute shader
    public Noise2D noise;           // noise function 
    public Material material;       // material of map

    public Vector3 scale = new Vector3(1, 1, 1);        // scale of map
    public Vector2 mapSize = new Vector2(2, 2);         // size of map in chunks
    public int maxChunkSize = 10000;                    // max chunk area
    public Vector2 chunkSize = new Vector2(100, 100);   // size of chunks
    public bool realtimeGeneration = false;             // update mesh as values are changed in the inspector

    int mcShaderID;                 // shader id
    RenderTexture heightmapBuffer;  // heightmap render texture
    ComputeBuffer vertexBuffer;     // vertex buffer passed to shader
    Vector3[] vertices;             // array of vertices
    ComputeBuffer indexBuffer;      // index buffer passed to shader
    int[] indices;                  // array of indices

    private void OnValidate()
    {
        mapSize = new Vector2(Mathf.Clamp(Mathf.Ceil(mapSize.x), 0, Mathf.Infinity), Mathf.Clamp(Mathf.Ceil(mapSize.y), 0, Mathf.Infinity));
        chunkSize = new Vector2(Mathf.Clamp(Mathf.Ceil(chunkSize.x), 0, Mathf.Infinity), Mathf.Clamp(Mathf.Ceil(chunkSize.y), 0, Mathf.Infinity));
    }

    /// <summary>
    /// Initialize index and vertex buffers
    /// </summary>
    public void InitBuffers()
    {
        // setup local arrays
        int maxTris = (int)chunkSize.x * (int)chunkSize.y;
        if (maxTris > maxChunkSize)
        {
            Debug.LogWarning("chunk size max reached!");
            maxTris = maxChunkSize;
        }
        vertices = new Vector3[maxTris * 6];
        indices = new int[maxTris * 6];

        // setup GPU buffers
        vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3, ComputeBufferType.Structured);
        indexBuffer = new ComputeBuffer(indices.Length, sizeof(int), ComputeBufferType.Structured);
    }

    /// <summary>
    /// Release all buffers and render textures
    /// </summary>
    public void ReleaseBuffers()
    {
        vertexBuffer.Release();
        indexBuffer.Release();
        RenderTexture.active = null;
        heightmapBuffer.Release();
    }

    /// <summary>
    /// Set up shader and link buffers
    /// </summary>
    public void InitShader()
    {
        mcShaderID = hmShader.FindKernel("MeshGen");
        hmShader.SetTexture(mcShaderID, "heightmap", heightmapBuffer);
        hmShader.SetBuffer(mcShaderID, "vertices", vertexBuffer);
        hmShader.SetBuffer(mcShaderID, "indices", indexBuffer);
        hmShader.SetFloats("scale", new float[] { scale.x, scale.y, scale.z });
        hmShader.SetInt("colCount", (int)chunkSize.x);
        hmShader.SetInt("rowCount", (int)chunkSize.y);
    }

    /// <summary>
    /// Dispatch shader and load data
    /// </summary>
    public void DispatchShader()
    {
        // get threadgroup sizes
        uint kx = 0, ky = 0, kz = 0;
        hmShader.GetKernelThreadGroupSizes(mcShaderID, out kx, out ky, out kz);
        hmShader.Dispatch(mcShaderID, (int)(chunkSize.x / kx) + 1, (int)(chunkSize.y / ky) + 1, 1);

        // get data from GPU
        vertexBuffer.GetData(vertices);
        indexBuffer.GetData(indices);
    }

    /// <summary>
    /// Generate 2D mesh chunks
    /// </summary>
    public void GenerateMesh()
    {

        InitBuffers();

        // get a list of child transforms
        List<Transform> children = new List<Transform>(gameObject.GetComponentsInChildren<Transform>(false));
        if (children.Contains(this.transform))
            children.Remove(this.transform);

        // iterate over x and y chunks
        int chunkIndex = 0;
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                chunkIndex = x + (int)mapSize.x * y;
                string name = string.Format("chunk ({0}, {1})", x, y);

                // make new chunk GameObject if one doesn't exist
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
                g.transform.position = transform.rotation * Vector3.Scale(new Vector3(x * chunkSize.x, y * chunkSize.y, 0), scale);

                // get heightmap
                int res = (int)Mathf.Max(chunkSize.x, chunkSize.y) + 1;
                Vector2 offset = new Vector2(chunkSize.x / (noise.scale.x * res), chunkSize.y / (noise.scale.y * res));
                heightmapBuffer = noise.CalculateNoise(noise.offset + new Vector2(x, y) * offset, noise.scale, res);
                InitShader();
                DispatchShader();

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
        ReleaseBuffers();

        // destroy any unused chunks
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