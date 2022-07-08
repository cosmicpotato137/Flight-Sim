using UnityEngine;
using System.IO;

public abstract class Noise2D : Noise
{
    public Vector2 offset;
    public Vector2 scale;

    public abstract RenderTexture CalculateNoise(Vector2 offset, Vector2 scale, int resolution);
    public override RenderTexture CalculateNoise()
    {
        return CalculateNoise(offset, scale, resolution);
    }

    public override void CreateShader()
    {
        string shaderName = "Scale2D";
        ComputeShader[] compShaders = (ComputeShader[])Resources.FindObjectsOfTypeAll(typeof(ComputeShader));
        for (int i = 0; i < compShaders.Length; i++)
        {
            if (compShaders[i].name == shaderName)
            {
                previewShader = compShaders[i];
                break;
            }
        }

        if (previewShader && previewShader.HasKernel("Scale2D"))
            previewHandle = previewShader.FindKernel("Scale2D");
    }

    // convert render texture to Texture2D
    public Texture2D GetNoiseTexture()
    {
        RenderTexture oldrt = RenderTexture.active;
        RenderTexture.active = CalculateNoise();
        Texture2D noiseTexture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        noiseTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        noiseTexture.Apply();
        RenderTexture.active = oldrt;
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
        if (!previewRT)
        {
            CreatePreviewRT();
        }

        if (previewShader && previewShader.HasKernel("Scale2D"))
        {
            previewShader.SetTexture(previewHandle, "Input", CalculateNoise());
            previewShader.SetTexture(previewHandle, "Result", previewRT);
            uint kx = 0, ky = 0, kz = 0;
            noiseShader.GetKernelThreadGroupSizes(previewHandle, out kx, out ky, out kz);
            previewShader.Dispatch(previewHandle, (int)(previewRes / kx) + 1, (int)(previewRes / ky) + 1, 1);
        }
    }

}
