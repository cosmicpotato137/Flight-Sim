using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Noise : ScriptableObject
{
    public ComputeShader noiseShader;
    protected int shaderHandle;

    protected ComputeShader previewShader;
    protected int previewHandle = -1;

    [HideInInspector]
    public int previewRes = 150;
    public int resolution;

    [HideInInspector]
    public RenderTexture previewRT;
    [HideInInspector]
    public bool showPreview;
    [HideInInspector]
    public bool realtime;

    private void OnEnable()
    {
        CreatePreviewRT();
        CreateShader();
        CalculatePreview();
    }

    private void OnValidate()
    {
        resolution = (int)Mathf.Clamp(resolution, 1, Mathf.Infinity);
        previewRes = (int)Mathf.Clamp(previewRes, 1, Mathf.Infinity);
    }

    public void CreatePreviewRT()
    {
        previewRes = 150;
        // create preview texture 
        previewRT = new RenderTexture(previewRes, previewRes, 0, RenderTextureFormat.ARGB32);
        previewRT.enableRandomWrite = true;
        previewRT.Create();
    }
    public abstract void CreateShader();
    public abstract void CalculatePreview();
    public abstract RenderTexture CalculateNoise();
    public abstract void SaveTexture();

}
