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
        previewRes = 150;
        // create preview texture 
        previewRT = new RenderTexture(previewRes, previewRes, 0, RenderTextureFormat.ARGB32);
        previewRT.enableRandomWrite = true;
        previewRT.Create();
        CreateShader();
        CalculatePreview();
    }

    private void OnValidate()
    {
        Mathf.Clamp(resolution, 1, Mathf.Infinity);
        Mathf.Clamp(previewRes, 1, Mathf.Infinity);
    }

    public abstract void CreateShader();
    public abstract void CalculatePreview();
    public abstract RenderTexture CalculateNoise();
    public abstract void SaveTexture();

}
