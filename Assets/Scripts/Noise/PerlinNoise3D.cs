using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New 3D Perlin Noise", menuName = "Noise/3D Perlin Noise")]
public class PerlinNoise3D : Noise3D
{
    public Vector3 offset;
    public Vector3 scale;
    public float weight;
    public uint seed;

    public override void CreateShader()
    {
        if (noiseShader)
            shaderHandle = noiseShader.FindKernel("PerlinNoise3D");
    }

    public override void CalculateNoise()
    {
        if (!(noiseShader && noiseShader.HasKernel("PerlinNoise3D") && noiseRT && noiseRT.IsCreated())) return;

        noiseShader.SetTexture(shaderHandle, "Result", noiseRT);
        noiseShader.SetFloats("scale", new float[] { scale.x, scale.y, scale.z });
        noiseShader.SetFloats("offset", new float[] { offset.x, offset.y, offset.z });
        noiseShader.SetFloat("noiseWeight", weight);
        noiseShader.SetInt("seed", (int)seed);
        scale = new Vector3(Mathf.Clamp(scale.x, 0, Mathf.Infinity), Mathf.Clamp(scale.y, 0, Mathf.Infinity), Mathf.Clamp(scale.z, 0, Mathf.Infinity));
        int numThreadGroups = Mathf.CeilToInt(resolution / 8.0f);
        noiseShader.Dispatch(shaderHandle, numThreadGroups, numThreadGroups, numThreadGroups);
    }

    public RenderTexture CalculateNoise(int resolution, Vector3 offset, Vector3 scale)
    {
        RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        rt.volumeDepth = resolution;
        rt.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        rt.enableRandomWrite = true;
        rt.Create();

        noiseShader.SetTexture(shaderHandle, "Result", rt);
        noiseShader.SetFloats("scale", new float[] { scale.x, scale.y, scale.z });
        noiseShader.SetFloats("offset", new float[] { offset.x, offset.y, offset.z });
        noiseShader.SetFloat("noiseWeight", weight);
        noiseShader.SetInt("seed", (int)seed);
        scale = new Vector3(Mathf.Clamp(scale.x, 0, Mathf.Infinity), Mathf.Clamp(scale.y, 0, Mathf.Infinity), Mathf.Clamp(scale.z, 0, Mathf.Infinity));
        int numThreadGroups = Mathf.CeilToInt(resolution / 8.0f);
        noiseShader.Dispatch(shaderHandle, numThreadGroups, numThreadGroups, numThreadGroups);

        return rt;
    }
}
