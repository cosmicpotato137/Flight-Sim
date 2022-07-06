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
    public ComputeShader mcShader;
    public PerlinNoise3D noise;
    int mcShaderID;

    public Vector3 scale;
    [Range(1.0f, 100000)]
    public int maxChunkSize = 1000;
    public int xdim = 5;
    public int ydim = 5;
    public int zdim = 5;
    public float threshold = .5f;
    public bool realtimeGeneration;

    RenderTexture heightmapBuffer;
    ComputeBuffer triangleBuffer;
    ComputeBuffer triCountBuffer;
    Triangle[] triangles;
    ComputeBuffer indexBuffer;
    int[] indices;
    ComputeBuffer debugBuffer;
    Matrix4x4[] debugs;

    int numTris;

    public void InitBuffers()
    {
        // setup local arrays
        int maxTris = xdim * ydim * zdim;
        if (maxTris > maxChunkSize)
        {
            Debug.LogWarning("chunk size max reached!");
            maxTris = maxChunkSize;
        }
        triangles = new Triangle[maxTris * 5];
        //debugs = new Matrix4x4[triCount];

        
        heightmapBuffer = noise.CalculateNoise();

        // setup GPU buffers
        triangleBuffer = new ComputeBuffer(triangles.Length, sizeof(float) * 9, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        //debugBuffer = new ComputeBuffer(debugs.Length, sizeof(float) * 4 * 4, ComputeBufferType.Structured);
    }

    public void ReleaseBuffers()
    {
        triCountBuffer.Release();
        triangleBuffer.Release();
        RenderTexture.active = null;
        heightmapBuffer.Release();
        //debugBuffer.Release();
    }

    public void InitShader()
    {
        // setup shader
        mcShaderID = mcShader.FindKernel("MeshGen");
        mcShader.SetTexture(mcShaderID, "noiseTexture", heightmapBuffer);
        triangleBuffer.SetCounterValue(0);
        mcShader.SetBuffer(mcShaderID, "triangles", triangleBuffer);
        //mcShader.SetBuffer(mcShaderID, "debug", debugBuffer);

        mcShader.SetFloats("scale", new float[] { scale.x, scale.y, scale.z });
        mcShader.SetInt("xdim", xdim);
        mcShader.SetInt("ydim", ydim);
        mcShader.SetInt("zdim", zdim);
        mcShader.SetFloat("isoLevel", threshold);
    }

    public void DispatchShader()
    {
        uint kx = 0, ky = 0, kz = 0;
        mcShader.GetKernelThreadGroupSizes(mcShaderID, out kx, out ky, out kz);
        mcShader.Dispatch(mcShaderID, (int)((float)xdim / (float)kx) + 1, (int)((float)ydim / (float)ky) + 1, (int)((float)zdim / (float)kz) + 1);

        // get data from GPU
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        numTris = triCountArray[0];
        triangleBuffer.GetData(triangles);
        //debugBuffer.GetData(debugs);

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
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
