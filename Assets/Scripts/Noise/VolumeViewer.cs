using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeViewer : MonoBehaviour
{
    public ComputeShader volumeSliceShader;

    public RenderTexture volume;
    public RenderTexture slice;

    public int layer = 6;
    public int axis = 0;

    private int slicerHandle;
    int numThreadGroups;

    private void OnValidate()
    {
        CreateSliceTexture();
    }

    public void Slice()
    {
        volumeSliceShader.SetTexture(slicerHandle, "result", slice);
        volumeSliceShader.SetTexture(slicerHandle, "volume", volume);
        volumeSliceShader.SetInt("layer", layer - 1);
        volumeSliceShader.SetInt("axis", axis);
        int numThreadGroups = Mathf.CeilToInt(100 / 8.0f);
        volumeSliceShader.Dispatch(slicerHandle, numThreadGroups, numThreadGroups, 1);
    }

    public void CreateSliceTexture()
    {
        slice = new RenderTexture(150, 150, 0, RenderTextureFormat.ARGB32);
        slice.enableRandomWrite = true;
        slice.Create();
    }

    public void ReleaseSliceTexture()
    {
        if (slice.IsCreated())
            slice.Release();
    }

    public void CreateSlicer()
    {
        slicerHandle = volumeSliceShader.FindKernel("Slicer");
    }
}
