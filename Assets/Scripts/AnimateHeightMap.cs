using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cosmicpotato.noisetools.Runtime;

[RequireComponent(typeof(HeightMap))]
public class AnimateHeightMap : MonoBehaviour
{
    HeightMap heightMap;
    NoiseAdder2D noiseAdder;
    public Vector2 velocity1;
    public Vector2 velocity2;
    public Vector2 velocity3;

    // Start is called before the first frame update
    void Start()
    {
        heightMap = gameObject.GetComponent<HeightMap>();
        noiseAdder = (NoiseAdder2D)heightMap.noise;
        heightMap.InitBuffers();
    }

    // Update is called once per frame
    void Update()
    {
        PerlinNoise2D p1 = (PerlinNoise2D)noiseAdder.noises[0];
        PerlinNoise2D p2 = (PerlinNoise2D)noiseAdder.noises[1];
        PerlinNoise2D p3 = (PerlinNoise2D)noiseAdder.noises[2];

        p1.offset += velocity1 * Time.deltaTime * Mathf.Cos(Time.deltaTime * .005f + 3);
        p2.offset += velocity2 * Time.deltaTime * Mathf.Cos(Time.deltaTime * .005f + 2);
        p3.offset += velocity3 * Time.deltaTime * Mathf.Cos(Time.deltaTime * .005f + 1);
        heightMap.GenerateMesh();
    }

    private void OnApplicationQuit()
    {
        heightMap.ReleaseBuffers();
    }
}
