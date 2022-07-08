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

    private void OnValidate()
    {
        xdim = (int)Mathf.Clamp(xdim, 1, Mathf.Infinity);
        ydim = (int)Mathf.Clamp(ydim, 1, Mathf.Infinity);

    }

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

    public void GenerateMesh()
    {
        InitBuffers();
        InitShader();
        DispatchShader();
        ReleaseBuffers();

        // make mesh
        if (!gameObject.GetComponent<MeshFilter>())
            gameObject.AddComponent<MeshFilter>();
        if (!gameObject.GetComponent<MeshRenderer>())
            gameObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.Optimize();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void ClearMesh()
    {
        GetComponent<MeshFilter>().mesh = new Mesh();
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateMesh();
    }

    // Update is called once per frame
    void Update()
    {

    }
}