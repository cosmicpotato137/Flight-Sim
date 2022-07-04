using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleMeshGen : MonoBehaviour
{
    public ComputeShader testShader;
    int testShaderHandle;

    public
    // Use this for initialization
    void Start()
    {
        // setup local arrays
        int rows = 5;
        int cols = 5;
        int triCount = rows * cols;
        Vector3[] verts = new Vector3[triCount * 3];
        int[] inds = new int[triCount * 3];
        int[] debugs = new int[10];

        // setup GPU buffers
        ComputeBuffer vertexBuffer = new ComputeBuffer(verts.Length, sizeof(float) * 3, ComputeBufferType.Structured);
        ComputeBuffer indexBuffer = new ComputeBuffer(inds.Length, sizeof(int), ComputeBufferType.Structured);
        ComputeBuffer debugBuffer = new ComputeBuffer(debugs.Length, sizeof(int), ComputeBufferType.Structured);

        // setup shader
        testShaderHandle = testShader.FindKernel("Test1");
        testShader.SetBuffer(testShaderHandle, "vertices", vertexBuffer);
        testShader.SetBuffer(testShaderHandle, "indices", indexBuffer);
        testShader.SetBuffer(testShaderHandle, "debug", debugBuffer);
        testShader.SetInt("colCount", cols);
        testShader.SetInt("rowCount", rows);
        testShader.Dispatch(testShaderHandle, 5, 5, 1);

        // get data from GPU
        vertexBuffer.GetData(verts);
        indexBuffer.GetData(inds);
        debugBuffer.GetData(debugs);
        vertexBuffer.Release();
        indexBuffer.Release();
        debugBuffer.Release();

        // make mesh
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = inds;

        // set material properties
        Renderer rend = GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("HDRP/Lit"));
        rend.material.color = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
