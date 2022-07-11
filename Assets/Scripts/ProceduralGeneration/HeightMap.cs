using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class HeightMap : MonoBehaviour
{
    public ComputeShader mcShader;
    public Noise2D noise;
    public Material material;

    public Vector3 scale;
    public Vector2 mapSize;
    public Vector2 chunkSize = new Vector2(100, 100);
    public bool realtimeGeneration;

    int mcShaderID;
    RenderTexture heightmapBuffer;
    ComputeBuffer vertexBuffer;
    Vector3[] vertices;
    ComputeBuffer indexBuffer;
    int[] indices;
    ComputeBuffer debugBuffer;
    Vector4[] debugs;

    List<Transform> children;

    private void Start()
    {
        GenerateMesh();
    }

    private void OnValidate()
    {
        mapSize = new Vector2(Mathf.Ceil(mapSize.x), Mathf.Ceil(mapSize.y));
        chunkSize = new Vector2(Mathf.Ceil(chunkSize.x), Mathf.Ceil(chunkSize.y));
    }

    public void InitBuffers()
    {
        // setup local arrays
        int triCount = (int)chunkSize.x * (int)chunkSize.y;
        vertices = new Vector3[triCount * 6];
        indices = new int[triCount * 6];
        debugs = new Vector4[triCount];

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
        mcShader.SetInt("colCount", (int)chunkSize.x);
        mcShader.SetInt("rowCount", (int)chunkSize.y);
    }

    public void DispatchShader()
    {
        uint kx = 0, ky = 0, kz = 0;
        mcShader.GetKernelThreadGroupSizes(mcShaderID, out kx, out ky, out kz);
        mcShader.Dispatch(mcShaderID, (int)(chunkSize.x / kx) + 1, (int)(chunkSize.y / ky) + 1, 1);

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
        List<Transform> children = new List<Transform>(gameObject.GetComponentsInChildren<Transform>(false));
        if (children.Contains(this.transform))
            children.Remove(this.transform);

        int chunkIndex = 0;
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                chunkIndex = x + (int)mapSize.x * y;
                string name = string.Format("chunk ({0}, {1})", x, y);
                GameObject g;
                // make chunk if it doesn't exist
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

                //Debug.Log(chunkIndex);
            }
        }
        ReleaseBuffers();

        for (int i = children.Count; i > chunkIndex + 1; i--)
        {
            DestroyImmediate(children[chunkIndex + 1].gameObject);
            children.RemoveAt(chunkIndex + 1);
        }
    }

    public void ClearMesh()
    {
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}