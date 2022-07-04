using UnityEngine;
using System.IO;

public abstract class Noise2D : Noise
{
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
        if (previewShader && previewShader.HasKernel("Scale2D"))
            previewHandle = previewShader.FindKernel("Scale2D");
    }

    public override void CreateNoiseRT()
    {
        resolution = (int)Mathf.Clamp(resolution, 1, Mathf.Infinity);
        noiseRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
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
        ReleaseNoiseRT();
        CreateNoiseRT();
        CalculateNoise();

        RenderTexture.active = noiseRT;
        Texture2D noiseTexture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        noiseTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        noiseTexture.Apply();
        RenderTexture.active = null;
        return noiseTexture;
    }

    public override void SaveTexture()
    {
        // RenderTexture -> Texture2D -> byte[]
        byte[] bytes = GetNoiseTexture().EncodeToPNG();
        Directory.CreateDirectory(Application.dataPath + "/Textures/");
        int count = 0;
        string countStr = "";
        while (File.Exists(Application.dataPath + "/Textures/NoiseTexture2D" + countStr + ".png"))
        {
            count++;
            countStr = "(" + count.ToString() + ")";
        }
        File.WriteAllBytes(Application.dataPath + "/Textures/NoiseTexture2D" + countStr + ".png", bytes);
    }

    public override void CalculatePreview()
    {
        CalculateNoise();

        if (previewShader && previewShader.HasKernel("Scale2D"))
        {
            int numThreadGroups = Mathf.CeilToInt(previewRes / 8.0f);
            previewShader.SetTexture(previewHandle, "Input", noiseRT); 
            previewShader.SetTexture(previewHandle, "Result", previewRT);
            previewShader.Dispatch(previewHandle, numThreadGroups, numThreadGroups, 1);
        }
    }

}
