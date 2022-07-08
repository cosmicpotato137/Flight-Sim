using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New 3D Perlin Noise", menuName = "Noise/3D Perlin Noise")]
public class PerlinNoise3D : Noise3D
{
    public float weight;
    public uint seed;

    public override void CreateShader()
    {
        base.CreateShader();
        if (noiseShader)
            shaderHandle = noiseShader.FindKernel("PerlinNoise3D");
    }

    public override RenderTexture CalculateNoise(Vector3 offset, Vector3 scale, int resolution)
    {
        RenderTexture result = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.volumeDepth = resolution;
        result.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        result.Create();

        if (!(noiseShader && noiseShader.HasKernel("PerlinNoise3D"))) return result;

        //scale = new Vector3(Mathf.Clamp(scale.x, 0, Mathf.Infinity), Mathf.Clamp(scale.y, 0, Mathf.Infinity), Mathf.Clamp(scale.z, 0, Mathf.Infinity));
        scale = scale * (float)resolution; // keep scale of noise constant with changing resolution
        noiseShader.SetTexture(shaderHandle, "Result", result);
        noiseShader.SetFloats("scale", new float[] { scale.x, scale.y, scale.z });
        noiseShader.SetFloats("offset", new float[] { offset.x, offset.y, offset.z });
        noiseShader.SetFloat("noiseWeight", weight);
        noiseShader.SetInt("seed", (int)seed);

        uint kx = 0, ky = 0, kz = 0;
        noiseShader.GetKernelThreadGroupSizes(shaderHandle, out kx, out ky, out kz);
        noiseShader.Dispatch(shaderHandle, (int)(resolution / kx) + 1, (int)(resolution / ky) + 1, (int)(resolution / kz) + 1);

        return result;
    }
}
