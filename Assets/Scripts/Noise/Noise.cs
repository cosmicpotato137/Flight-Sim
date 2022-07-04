using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Noise : ScriptableObject
{
    public ComputeShader noiseShader;
    protected int shaderHandle;

    public ComputeShader previewShader;
    protected int previewHandle;
    
    [HideInInspector]
    public int previewRes = 150;
    public int resolution;

    [HideInInspector]
    public RenderTexture noiseRT;
    [HideInInspector]
    public RenderTexture previewRT;

    public abstract void CreateShader();
    public abstract void CreateNoiseRT();
    public abstract void ReleaseNoiseRT();
    public abstract void CalculateNoise();
    public abstract void CalculatePreview();
    public abstract void SaveTexture();

}
