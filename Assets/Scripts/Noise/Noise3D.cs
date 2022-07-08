using UnityEngine;
using System.IO;

public abstract class Noise3D : Noise
{
    public Vector3 offset;
    public Vector3 scale;
    [HideInInspector]
    public int axis;
    [HideInInspector]
    public int layer;

    public abstract RenderTexture CalculateNoise(Vector3 offset, Vector3 scale, int resolution);
    public override RenderTexture CalculateNoise()
    {
        return CalculateNoise(offset, scale, resolution);
    }

    public override void CreateShader()
    {
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

    // convert render texture to Texture2D
    public Texture2D GetNoiseTexture()
    {
        RenderTexture.active = CalculateNoise();
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
        if (!previewRT)
        {
            CreatePreviewRT();
        }
        if (previewHandle != -1)
        {
            previewShader.SetTexture(previewHandle, "Volume", CalculateNoise());
            previewShader.SetTexture(previewHandle, "Result", previewRT);
            previewShader.SetInt("axis", axis);
            previewShader.SetInt("layer", layer-1);

            uint kx = 0, ky = 0, kz = 0;
            previewShader.GetKernelThreadGroupSizes(previewHandle, out kx, out ky, out kz);
            previewShader.Dispatch(previewHandle, (int)(previewRes / kx) + 1, (int)(previewRes / ky) + 1, (int)(previewRes / kz) + 1);
        }
    }

    public RenderTexture Slice(int axis, int layer)
    {
        RenderTexture result = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.Create();

        if (previewShader && previewShader.HasKernel("Slicer"))
        {
            previewShader.SetTexture(previewHandle, "Volume", CalculateNoise());
            previewShader.SetTexture(previewHandle, "Result", result);
            axis = Mathf.Clamp(axis, 0, 2);
            previewShader.SetInt("axis", axis);
            layer = Mathf.Clamp(layer, 0, resolution - 1);
            previewShader.SetInt("layer", layer);

            uint kx = 0, ky = 0, kz = 0;
            noiseShader.GetKernelThreadGroupSizes(previewHandle, out kx, out ky, out kz);
            noiseShader.Dispatch(previewHandle, (int)(previewRes / kx) + 1, (int)(previewRes / ky) + 1, (int)(previewRes / kz) + 1);
        }

        return result;
    }

}
