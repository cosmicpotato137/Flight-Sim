using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for the noise system
/// </summary>
public abstract class Noise : ScriptableObject
{
    public ComputeShader noiseShader;   // noise shader
    public int resolution = 20;         // resolution of texture
    public bool realtime = false;       // preview in realtime


    protected ComputeShader previewShader;  // preview shader
    protected int shaderHandle = -1;        // shader id
    protected int previewHandle = -1;       // preview shader id

    [HideInInspector] public RenderTexture previewRT;   // preview render texture
    [HideInInspector] public int previewRes = 150;      // preview resolution
    [HideInInspector] public bool showPreview;          // show preview dropdown

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

    /// <summary>
    /// Initialize preview render texture
    /// </summary>
    public void CreatePreviewRT()
    {
        previewRes = 150;
        // create preview texture 
        previewRT = new RenderTexture(previewRes, previewRes, 0, RenderTextureFormat.ARGB32);
        previewRT.enableRandomWrite = true;
        previewRT.Create();
    }

    /// <summary>
    /// Initialize shaders
    /// </summary>
    public abstract void CreateShader();
    /// <summary>
    /// Convert noise to preview render texture
    /// </summary>
    public abstract void CalculatePreview();
    /// <returns>Calculated noise texture</returns>
    public abstract RenderTexture CalculateNoise();
    /// <summary>
    /// Save noise texture to png
    /// </summary>
    public abstract void SaveTexture();

}
