using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "New 2D Noise Adder", menuName = "Noise/2D Noise Adder")]
public class NoiseAdder2D : Noise2D
{
    [SerializeField]
    public List<Noise2D> noises;

    public override void CreateNoiseRT()
    {
        base.CreateNoiseRT();
        for (int i = 0; i < noises.Count; i++)
        {
            noises[i].CreateNoiseRT();
        }
    }

    public override void ReleaseNoiseRT()
    {
        base.ReleaseNoiseRT();
        for (int i = 0; i < noises.Count; i++)
        {
            noises[i].ReleaseNoiseRT();
        }
    }

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
            else
                Debug.LogWarning("Filter not recognized");
        }
    }

    public override void CalculateNoise()
    {
        if (noiseShader && noises.Count > 0)
        {
            int i = 0;
            Noise2D noise1 = null;
            Noise2D noise2 = null;
            while (i < noises.Count)
            {
                if (noises[i] && noises[i].noiseRT && noises[i].noiseRT.IsCreated())
                {
                    if (!noise1)
                        noise1 = noises[i];
                    else if (!noise2)
                        noise2 = noises[i];
                    else
                        break;
                }
                i++;
            }

            if (noise1)
            {
                noise1.CalculateNoise();
                noise1 = ResetResolution(noise1);
                noiseRT = noise1.noiseRT;
            }
            if (!noise2) return;

            noise2.CalculateNoise();
            noise2 = ResetResolution(noise2);
            noiseRT = AddNoise(noise2.noiseRT, noise1.noiseRT, resolution);

            while (i < noises.Count)
            {
                if (noises[i] && noises[i].noiseRT && noises[i].noiseRT.IsCreated()) continue;
                noises[i].CalculateNoise();
                noises[i] = ResetResolution(noises[i]);
                noiseRT = AddNoise(noises[i].noiseRT, noiseRT, resolution);
            }
        }
    }

    private RenderTexture AddNoise(RenderTexture input, RenderTexture result, int resolution)
    {
        noiseShader.SetTexture(shaderHandle, "Input", input);
        noiseShader.SetTexture(shaderHandle, "Result", result);
        int numThreadGroups = Mathf.CeilToInt((float)resolution / 8.0f);
        noiseShader.Dispatch(shaderHandle, numThreadGroups, numThreadGroups, 1);
        return result;
    }

    private Noise2D ResetResolution(Noise2D input)
    {
        if (input.resolution != resolution)
        {
            input.resolution = resolution;
            input.ReleaseNoiseRT();
            input.CreateNoiseRT();
        }
        return input;
    }
}
