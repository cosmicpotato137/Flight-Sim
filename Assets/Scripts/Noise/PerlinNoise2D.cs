using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Perlin Noise", menuName = "Noise/2D Perlin Noise")]
public class PerlinNoise2D : Noise2D
{
    public int seed;
    [Range(0f, 1f)]
    public float alpha;
    public float weight;

    public override void CreateShader()
    {
        base.CreateShader();
        if (noiseShader && noiseShader.HasKernel("PerlinNoise2D"))
            shaderHandle = noiseShader.FindKernel("PerlinNoise2D");
    }

    public override RenderTexture CalculateNoise(Vector2 offset, Vector2 scale, int resolution)
    {
        RenderTexture result = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.Create();

        if (noiseShader && noiseShader.HasKernel("PerlinNoise2D"))
        {
            scale = scale * (float)resolution / 5.0f; // keep scale of noise constant with changing resolution
            noiseShader.SetTexture(shaderHandle, "Result", result);
            noiseShader.SetFloats("scale", new float[]{ scale.x, scale.y });
            noiseShader.SetFloats("offset", new float[]{ offset.x, offset.y });
            noiseShader.SetFloat("noiseWeight", weight);
            noiseShader.SetInt("seed", (int)seed);
            noiseShader.SetFloat("alpha", alpha);

            uint kx = 0, ky = 0, kz = 0;
            noiseShader.GetKernelThreadGroupSizes(shaderHandle, out kx, out ky, out kz);
            noiseShader.Dispatch(shaderHandle, (int)(resolution / kx) + 1, (int)(resolution / ky) + 1, 1);
        }

        return result;
    }

    public override RenderTexture CalculateNoise()
    {
        return CalculateNoise(offset, scale, resolution);
    }
}
