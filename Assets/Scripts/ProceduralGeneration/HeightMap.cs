using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class HeightMap : MonoBehaviour
{
    public ComputeShader mcShader;
    public Noise2D noise;
    int mcShaderID;

    public Vector3 scale;
    public int xdim;
    public int ydim;
    public bool realtimeGeneration;

    RenderTexture heightmapBuffer;
    ComputeBuffer vertexBuffer;
    Vector3[] vertices;
    ComputeBuffer indexBuffer;
    int[] indices;
    ComputeBuffer debugBuffer;
    Vector4[] debugs;

    Material material;

    //List<Transform> children = new List<Transform>();
    [HideInInspector] public Dictionary<int, Transform> children;

    private void OnEnable()
    {
        // get a list of child transforms
        //children = new List<Transform>(gameObject.GetComponentsInChildren<Transform>(false));
        //if (children.Contains(this.transform))
        //    children.Remove(this.transform);

        ClearMesh();
        GenerateMesh();
    }

    private void Reset()
    {
        // get a list of child transforms
        //children = new List<Transform>(gameObject.GetComponentsInChildren<Transform>(false));
        //if (children.Contains(this.transform))
        //    children.Remove(this.transform);

        ClearMesh();
        GenerateMesh();
    }

    private void OnValidate()
    {
        xdim = (int)Mathf.Clamp(xdim, 1, Mathf.Infinity);
        ydim = (int)Mathf.Clamp(ydim, 1, Mathf.Infinity);

    }

    public Vector3 MapScale()
    {
        return Vector3.Scale(transform.localScale, scale);
    }

    /// <summary>
    /// Initialize index and vertex buffers
    /// </summary>
    public void InitBuffers()
    {
        // setup local arrays
        int triCount = xdim * ydim;
        vertices = new Vector3[triCount * 6];
        indices = new int[triCount * 6];
        debugs = new Vector4[triCount];

        // get heightmap
        heightmapBuffer = noise.CalculateNoise(noise.offset, noise.scale, Mathf.Max(xdim, ydim));

        // setup GPU buffers
        vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3, ComputeBufferType.Structured);
        indexBuffer = new ComputeBuffer(indices.Length, sizeof(int), ComputeBufferType.Structured);
        debugBuffer = new ComputeBuffer(debugs.Length, sizeof(float) * 4, ComputeBufferType.Structured);
    }

    public void ReleaseBuffers()
    {
        vertexBuffer.Release();
        indexBuffer.Release();
        RenderTexture.active = null;
        heightmapBuffer.Release();
        debugBuffer.Release();
    }

    public void InitShader()
    {
        // setup shader
        mcShaderID = mcShader.FindKernel("MeshGen");
        mcShader.SetTexture(mcShaderID, "heightmap", heightmapBuffer);
        mcShader.SetBuffer(mcShaderID, "vertices", vertexBuffer);
        mcShader.SetBuffer(mcShaderID, "indices", indexBuffer);
        mcShader.SetBuffer(mcShaderID, "debug", debugBuffer);
        mcShader.SetFloats("scale", new float[] { scale.x, scale.y, scale.z });
        mcShader.SetInt("colCount", xdim);
        mcShader.SetInt("rowCount", ydim);
    }

    public void DispatchShader()
    {

        uint kx = 0, ky = 0, kz = 0;
        mcShader.GetKernelThreadGroupSizes(mcShaderID, out kx, out ky, out kz);
        mcShader.Dispatch(mcShaderID, (int)(xdim / kx) + 1, (int)(ydim / ky) + 1, 1);

        // get data from GPU
        vertexBuffer.GetData(vertices);
        indexBuffer.GetData(indices);
        debugBuffer.GetData(debugs);

        //for (int i = 0; i < debugs.Length; i++)
        //{
        //    print(debugs[i]);
        //}
    }

    public int SetChunk(int x, int y)
    {
        int chunkIndex = x + (int)mapSize.x * y;
        string name = string.Format("chunk ({0}, {1})", x, y);

        // make new chunk GameObject if one doesn't exist
        GameObject g;
        if (!children.ContainsKey(chunkIndex))
        {
            g = new GameObject(name);
            g.transform.SetParent(transform);
            g.AddComponent<MeshFilter>();
            g.AddComponent<MeshRenderer>();
            children.Add(chunkIndex, g.transform);
        }
        else
        {
            g = children[chunkIndex].gameObject;
            g.name = name;
        }

        // set transform
        g.transform.rotation = transform.rotation;
        g.transform.position = transform.rotation * Vector3.Scale(Vector3.Scale(new Vector3(x * chunkSize.x, y * chunkSize.y, 0), scale), transform.localScale);
        g.transform.position += transform.position;
        g.transform.localScale = new Vector3(1, 1, 1);

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

        return chunkIndex;
    }

    /// <summary>
    /// Generate 2D mesh chunks
    /// </summary>
    public void GenerateMesh()
    {
        InitBuffers();
        // iterate over x and y chunks
        int chunkIndex = 0;
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                chunkIndex = SetChunk(x, y);
            }
        }
        ReleaseBuffers();

        // destroy any unused chunks
        int childCount = children.Count;
        for (int i = chunkIndex + 1; i < childCount; i++)
        {
            if (children.ContainsKey(i) && children[i] != null)
                DestroyImmediate(children[i].gameObject);
            children.Remove(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (children != null)
            children.Clear();
        else 
            children = new Dictionary<int, Transform>();
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
}