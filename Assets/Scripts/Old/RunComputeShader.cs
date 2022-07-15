using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunComputeShader : MonoBehaviour
{
    public ComputeShader shader;
    public ComputeShader slicer;
    public int resolution = 100;

    public Color color;
    public int layer = 6;
    public int axis = 0;

    public RenderTexture slice;
    public RenderTexture tex;
    private int shaderHandle;
    private int slicerHandle;

    int numThreadGroups;


    public void OnValidate()
    {
        shaderHandle = shader.FindKernel("CSMain");
        slicerHandle = slicer.FindKernel("Slicer");
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Run()
    {
        shader.SetTexture(shaderHandle, "volume", tex);
        shader.SetFloats("color", new float[] { color.r, color.g, color.b, color.a });
        int numThreadGroups = Mathf.CeilToInt(resolution / 8.0f);
        shader.Dispatch(shaderHandle, numThreadGroups, numThreadGroups, numThreadGroups);
    }

    public void Slice()
    {
        slicer.SetTexture(slicerHandle, "result", slice);
        slicer.SetTexture(slicerHandle, "volume", tex);
        slicer.SetInt("layer", layer);
        slicer.SetInt("axis", axis);
        int numThreadGroups = Mathf.CeilToInt(100 / 8.0f);
        slicer.Dispatch(slicerHandle, numThreadGroups, numThreadGroups, 1);
    }

    public void CreateTextures()
    {
        resolution = (int)Mathf.Clamp(resolution, 1, Mathf.Infinity);
        layer = (int)Mathf.Clamp(layer, 0, resolution - 1);
        tex = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        tex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        tex.volumeDepth = resolution;
        tex.enableRandomWrite = true;
        tex.Create();

        slice = new RenderTexture(100, 100, 0, RenderTextureFormat.ARGB32);
        slice.enableRandomWrite = true;
        slice.Create();
    }

    public void ReleaseTextures()
    {
        if (tex.IsCreated())
            tex.Release();
        if (slice.IsCreated())
            slice.Release();
    }
}
