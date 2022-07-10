using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New 3D Noise Adder", menuName = "Noise/3D Noise Adder")]
public class NoiseAdder3D : Noise3D
{
    [SerializeField]
    public List<Noise3D> noises;

    public override void CreateShader()
    {
        base.CreateShader();
        if (noiseShader)
        {
            if (noiseShader.HasKernel("Filter3D"))
                shaderHandle = noiseShader.FindKernel("Filter3D");
            else
                Debug.LogError("Filter not recognized");
        }
    }

    public override RenderTexture CalculateNoise(Vector3 offset, Vector3 scale, int resolution)
    {
        RenderTexture result = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.volumeDepth = resolution;
        result.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        result.Create();

        if (noiseShader && noises.Count > 0)
        {
            for (int i = 0; i < noises.Count; i++)
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
                RenderTexture rt = noises[i].CalculateNoise(
                    noises[i].offset + new Vector3(offset.x / noises[i].scale.x, offset.y / noises[i].scale.y, offset.z / noises[i].scale.z), 
                    Vector3.Scale(noises[i].scale, scale), resolution);
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
        noiseShader.Dispatch(shaderHandle, (int)(resolution / kx) + 1, (int)(resolution / ky) + 1, (int)(resolution / kx) + 1);
        return result;
    }
}
