using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Perlin Noise", menuName = "Noise/2D Perlin Noise")]
public class PerlinNoise2D : Noise2D
{
    public int seed;
    public Vector2 offset;
    public Vector2 scale;
    public float alpha;
    public float weight;

    public override void CreateShader()
    {
        base.CreateShader();
        if (noiseShader && noiseShader.HasKernel("PerlinNoise2D"))
            shaderHandle = noiseShader.FindKernel("PerlinNoise2D");
    }

    public override void CalculateNoise()
    {
        if (noiseShader && noiseShader.HasKernel("PerlinNoise2D"))
        {
            noiseShader.SetTexture(shaderHandle, "Result", noiseRT);
            noiseShader.SetFloats("scale", new float[]{ scale.x, scale.y });
            noiseShader.SetFloats("offset", new float[]{ offset.x, offset.y });
            noiseShader.SetFloat("noiseWeight", weight);
            noiseShader.SetInt("seed", (int)seed);
            noiseShader.SetFloat("alpha", alpha);
            scale = new Vector2(Mathf.Clamp(scale.x, 0, Mathf.Infinity),Mathf.Clamp(scale.y, 0, Mathf.Infinity));
            int numThreadGroups = Mathf.CeilToInt(resolution / 8.0f);
            noiseShader.Dispatch(shaderHandle, numThreadGroups, numThreadGroups, 1);
        }
    }
}
