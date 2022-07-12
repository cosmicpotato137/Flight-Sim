using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Opperates on a list of noises
/// </summary>
[CreateAssetMenu(fileName = "New 2D Noise Adder", menuName = "Noise/2D Noise Adder")]
public class NoiseAdder2D : Noise2D
{
    [SerializeField]
    public List<Noise2D> noises;

    public override void CreateShader()
    {
        base.CreateShader();
        if (noiseShader)
        {
            if (noiseShader.HasKernel("Add2D"))
                shaderHandle = noiseShader.FindKernel("Add2D");
            else if (noiseShader.HasKernel("Multiply2D"))
                shaderHandle = noiseShader.FindKernel("Multiply2D");
            else if (noiseShader.HasKernel("WeightedBlend2D"))
                shaderHandle = noiseShader.FindKernel("WeightedBlend2D");
            else if (noiseShader.HasKernel("Filter2D"))
                shaderHandle = noiseShader.FindKernel("Filter2D");
            else
                Debug.LogWarning("Filter not recognized");
        }
    }

    public override RenderTexture CalculateNoise(Vector2 offset, Vector2 scale, int resolution)
    {
        RenderTexture result = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.Create();

        if (noiseShader && noises.Count > 0)
        {
            for (int i = 0;  i < noises.Count; i++)
            {
                if (!noises[i]) continue;
                if (noises[i] == this)
                {
                    noises.Remove(noises[i]);
                    Debug.Log("Cannot add this noise adder to itself");
                    i--;
                    continue;
                }
                noises[i].resolution = resolution;
                RenderTexture rt = noises[i].CalculateNoise(noises[i].offset + offset / noises[i].scale, noises[i].scale * scale, resolution);
                result = AddNoise(rt, result, resolution);
            }
        }

        return result;
    }

    private RenderTexture AddNoise(RenderTexture input, RenderTexture result, int resolution)
    {
        noiseShader.SetTexture(shaderHandle, "Input", input);
        noiseShader.SetTexture(shaderHandle, "Result", result);
        uint kx, ky, kz;
        noiseShader.GetKernelThreadGroupSizes(shaderHandle, out kx, out ky, out kz);
        noiseShader.Dispatch(shaderHandle, (int)(resolution / kx) + 1, (int)(resolution / ky) + 1, 1);
        return result;
    }
}
