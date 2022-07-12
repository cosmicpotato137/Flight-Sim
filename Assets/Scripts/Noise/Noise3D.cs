using UnityEngine;
using System.IO;

/// <summary>
/// 3D noise base class
/// </summary>
public abstract class Noise3D : Noise
{
    public Vector3 offset = new Vector3(0, 0, 0);   // offset of noise texture
    public Vector3 scale = new Vector3(1, 1, 1);    // scale of noise

    [HideInInspector] public int axis;  // preview axis
    [HideInInspector] public int layer; // preview layer

    /// <param name="offset"></param>
    /// <param name="scale"></param>
    /// <param name="resolution"></param>
    /// <returns>Calculated noise texture</returns>
    public abstract RenderTexture CalculateNoise(Vector3 offset, Vector3 scale, int resolution);
    public override RenderTexture CalculateNoise()
    {
        return CalculateNoise(offset, scale, resolution);
    }
    
    public override void CreateShader()
    {
        // search all assets for the right shader
        string shaderName = "SliceVolume";
        ComputeShader[] compShaders = (ComputeShader[])Resources.FindObjectsOfTypeAll(typeof(ComputeShader));
        for (int i = 0; i < compShaders.Length; i++)
        {
            if (compShaders[i].name == shaderName)
            {
                previewShader = compShaders[i];
                break;
            }
        }

        if (previewShader && previewShader.HasKernel("Slicer"))
            previewHandle = previewShader.FindKernel("Slicer");
    }

    /// <summary> 
    /// convert render texture to Texture2D
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public Texture2D GetNoiseTexture()
    {
        throw new System.NotImplementedException();
        //RenderTexture.active = CalculateNoise();
        //Texture2D noiseTexture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        //noiseTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        //noiseTexture.Apply();
        //RenderTexture.active = null;
        //return noiseTexture;
    }

    /// <summary>
    /// Save RenderTexture to png
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void SaveTexture()
    {
        throw new System.NotImplementedException();
    }

    public override void CalculatePreview()
    {
        if (!previewRT)
        {
            CreatePreviewRT();
        }
        if (previewHandle != -1)
        {
            // set shader vals
            previewShader.SetTexture(previewHandle, "Volume", CalculateNoise());
            previewShader.SetTexture(previewHandle, "Result", previewRT);
            previewShader.SetInt("axis", axis);
            previewShader.SetInt("layer", layer-1);

            // get threadgroups
            uint kx = 0, ky = 0, kz = 0;
            previewShader.GetKernelThreadGroupSizes(previewHandle, out kx, out ky, out kz);
            previewShader.Dispatch(previewHandle, (int)(previewRes / kx) + 1, (int)(previewRes / ky) + 1, (int)(previewRes / kz) + 1);
        }
    }

    /// <summary>
    /// Get a layer of 3D nosise 
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="layer"></param>
    /// <returns>Noise render texture</returns>
    public RenderTexture Slice(int axis, int layer)
    {
        // create render texture
        RenderTexture result = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.Create();

        if (previewShader && previewShader.HasKernel("Slicer"))
        {
            // set shader vals
            previewShader.SetTexture(previewHandle, "Volume", CalculateNoise());
            previewShader.SetTexture(previewHandle, "Result", result);
            axis = Mathf.Clamp(axis, 0, 2);
            previewShader.SetInt("axis", axis);
            layer = Mathf.Clamp(layer, 0, resolution - 1);
            previewShader.SetInt("layer", layer);

            // get threadgroups
            uint kx = 0, ky = 0, kz = 0;
            noiseShader.GetKernelThreadGroupSizes(previewHandle, out kx, out ky, out kz);
            noiseShader.Dispatch(previewHandle, (int)(previewRes / kx) + 1, (int)(previewRes / ky) + 1, (int)(previewRes / kz) + 1);
        }

        return result;
    }

}
