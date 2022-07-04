using UnityEngine;
using System.IO;

public abstract class Noise3D : Noise
{
    [HideInInspector]
    public int axis;
    [HideInInspector]
    public int layer;

    private void OnValidate()
    {
        previewRes = 100;
        // create preview texture 
        previewRT = new RenderTexture(previewRes, previewRes, 0, RenderTextureFormat.ARGB32);
        previewRT.enableRandomWrite = true;
        previewRT.Create();

        CreateShader();
    }

    public override void CreateShader()
    {
        if (previewShader && previewShader.HasKernel("Slicer"))
            previewHandle = previewShader.FindKernel("Slicer");
    }

    public override void CreateNoiseRT()
    {
        resolution = (int)Mathf.Clamp(resolution, 1, Mathf.Infinity);
        noiseRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        noiseRT.volumeDepth = resolution;
        noiseRT.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        noiseRT.enableRandomWrite = true;
        noiseRT.Create();
    }

    public override void ReleaseNoiseRT()
    {
        if (noiseRT && noiseRT.IsCreated())
            noiseRT.Release();
    }

    // convert render texture to Texture2D
    public Texture2D GetNoiseTexture()
    {
        CalculateNoise();

        RenderTexture.active = noiseRT;
        Texture2D noiseTexture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        noiseTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        noiseTexture.Apply();
        RenderTexture.active = null;
        return noiseTexture;
    }

    /*    public override void SaveTexture()
        {
            // RenderTexture -> Texture2D -> byte[]
            byte[] bytes = GetNoiseTexture().EncodeToPNG();
            Directory.CreateDirectory(Application.dataPath + "/Textures/");
            int count = 0;
            string countStr = "";
            while (File.Exists(Application.dataPath + "/Textures/NoiseTexture3D" + countStr + ".png"))
            {
                count++;
                countStr = "(" + count.ToString() + ")";
            }
            File.WriteAllBytes(Application.dataPath + "/Textures/NoiseTexture3D" + countStr + ".png", bytes);
        }*/

    public override void SaveTexture()
    {
        throw new System.NotImplementedException();
    }

    public override void CalculatePreview()
    {
        if (previewShader && previewShader.HasKernel("Slicer"))
        {
            int numThreadGroups = Mathf.CeilToInt(previewRes / 8.0f);
            previewShader.SetTexture(previewHandle, "Volume", noiseRT);
            previewShader.SetTexture(previewHandle, "Result", previewRT);
            previewShader.SetInt("axis", axis);
            previewShader.SetInt("layer", layer-1);
            previewShader.Dispatch(previewHandle, numThreadGroups, numThreadGroups, 1);
        }
    }

    public RenderTexture Slice(int axis, int layer)
    {
        RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();

        if (previewShader && previewShader.HasKernel("Slicer"))
        {
            int numThreadGroups = Mathf.CeilToInt(previewRes / 8.0f);
            previewShader.SetTexture(previewHandle, "Volume", noiseRT);
            previewShader.SetTexture(previewHandle, "Result", rt);
            axis = Mathf.Clamp(axis, 0, 2);
            previewShader.SetInt("axis", axis);
            layer = Mathf.Clamp(layer, 0, resolution - 1);
            previewShader.SetInt("layer", layer);
            previewShader.Dispatch(previewHandle, numThreadGroups, numThreadGroups, 1);
        }

        return rt;
    }

}
