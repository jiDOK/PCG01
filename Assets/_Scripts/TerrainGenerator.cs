using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] KeyCode kc_GenerateTerrain = KeyCode.Space;
    [SerializeField] Texture2D heightTex;
    [SerializeField, Range(1, 8)] int octaves = 1;
    [SerializeField] float lacunarity = 2f;
    [SerializeField, Range(0, 1)] float gain = 0.5f;// must be between 0 and 1, so it only attenuates
    [SerializeField] float perlinScale = 10f;
    float realPerlinScale = 0.1f;
    [SerializeField, Range(0f, 3f)] float heightScale = 0.4f;
    Terrain terrain;
    NoiseGenerator noise;

    void Start()
    {
        terrain = GetComponent<Terrain>();
        realPerlinScale = perlinScale * 0.01f;
    }

    void Update()
    {
        realPerlinScale = perlinScale * 0.01f;
        if (Input.GetKeyDown(kc_GenerateTerrain))
        {
            noise = new NoiseGenerator(octaves, lacunarity, gain, realPerlinScale);
            int res = terrain.terrainData.heightmapResolution;
            float[,] heights = new float[res, res];
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float hMapFactor = 1f;
                    if (heightTex != null)
                    {
                        int texX = Mathf.RoundToInt((float)x / res * heightTex.width);
                        int texY = Mathf.RoundToInt((float)y / res * heightTex.height);
                        hMapFactor = heightTex.GetPixel(texX, texY).r;
                    }
                    //heights[x, y] = noise.GetFractalNoise(x, y) * heightScale * (heightTex == null ? 1 : heightTex.GetPixel(x, y).r);
                    heights[y, x] = noise.GetFractalNoise(x, y) * heightScale * hMapFactor;
                }
            }
            terrain.terrainData.SetHeights(0, 0, heights);
        }
    }
}
