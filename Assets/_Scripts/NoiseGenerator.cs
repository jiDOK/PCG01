using UnityEngine;

public class NoiseGenerator
{
    int octaves;
    float lacunarity;
    float gain;
    float perlinScale;
    float xOffset = 100;
    float zOffset = 100;

    public NoiseGenerator() { }

    public NoiseGenerator(int octaves, float lacunarity, float gain, float perlinScale)
    {
        this.octaves = octaves;
        this.lacunarity = lacunarity;
        this.gain = gain;
        this.perlinScale = perlinScale;
    }

    public float GetValueNoise()
    {
        return Random.value;
    }

    public float GetPerlinNoise(float x, float z)
    {
        return Mathf.PerlinNoise((x + xOffset) * perlinScale, (z + zOffset) * perlinScale);
    }

    public float GetFractalNoise(float x, float z)
    {
        float fractalNoise = 0;
        float frequency = 1;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            float xVal = x * frequency;
            float zVal = z * frequency;
            fractalNoise += amplitude * GetPerlinNoise(xVal, zVal);
            frequency *= lacunarity;
            amplitude *= gain;
        }
        return fractalNoise;
    }
}
