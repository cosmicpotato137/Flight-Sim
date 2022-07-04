using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesExp : MonoBehaviour
{

    const int threadGroupSize = 8;

    [Header("General Settings")]
    public ComputeShader shader;
    public PerlinNoise3D noiseGenerator;

    public bool autoUpdateInEditor = true;
    public bool autoUpdateInGame = true;
    public Material mat;
    public bool generateColliders;

    [Header("Voxel Settings")]
    public int numPointsPerAxis;
    public float isoLevel;
    public Vector3 offset = Vector3.zero;

    ComputeBuffer triangleBuffer;
    ComputeBuffer triCountBuffer;

    bool settingsUpdated;

    private MeshFilter meshFilter;
    private Mesh mesh;
    private int shaderHandle;

    void Awake()
    {
        shaderHandle = shader.FindKernel("March");
        CreateBuffers();
    }

    void Update()
    {
    }

    public void UpdateMesh()
    {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)threadGroupSize);

        RenderTexture rt = new RenderTexture(noiseGenerator.CalculateNoise(numPointsPerAxis, new Vector3(0, 0, 0), new Vector3(1, 1, 1)));

        triangleBuffer.SetCounterValue(0);
        shader.SetTexture(shaderHandle, "points", rt);
        shader.SetBuffer(shaderHandle, "triangles", triangleBuffer);
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("isoLevel", isoLevel);

        shader.Dispatch(shaderHandle, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader 
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.Clear();

        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    void OnDestroy()
    {
        ReleaseBuffers();
    }

    public void CreateBuffers()
    {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }

    void ReleaseBuffers()
    {
        if (triangleBuffer != null)
        {
            triangleBuffer.Release();
            triCountBuffer.Release();
        }
    }
    
    void OnValidate()
    {
        settingsUpdated = true;
    }

    struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

/*    void OnDrawGizmos()
    {
        if (showBoundsGizmo)
        {
            Gizmos.color = boundsGizmoCol;

            List<Chunk> chunks = (this.chunks == null) ? new List<Chunk>(FindObjectsOfType<Chunk>()) : this.chunks;
            foreach (var chunk in chunks)
            {
                Bounds bounds = new Bounds(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
                Gizmos.color = boundsGizmoCol;
                Gizmos.DrawWireCube(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
            }
        }
    }*/

}