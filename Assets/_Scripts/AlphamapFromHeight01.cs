using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class AlphamapFromHeight01 : MonoBehaviour
{
    [SerializeField] KeyCode kc_PaintOnAlphamap = KeyCode.Space;
    [SerializeField] List<TextureData> textureAttributes = new List<TextureData>();
    Terrain terrain;
    TerrainData data;
    // leeres 3-dimensionales Array mit Indexen, die x -Koordinate, y Koordinate, und Texture Layer entspreichen.
    // es wird ein float-Wert gespeichert, der die Gewichtung bestimmt.
    float[,,] alphamap;
    // eine Textur muss herhalten, wenn keine andere will (Gewichtung von allen anderen 0 ist)
    int indexOfDefaultTexture;
    float maxHeight;

    void Start()
    {
        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;
        maxHeight = GetMaxHeight(data, data.heightmapWidth);
        alphamap = new float[data.alphamapWidth, data.alphamapHeight, data.alphamapLayers];
        for (int i = 0; i < textureAttributes.Count; i++)
        {
            if (textureAttributes[i].defaultTexture)
            {
                indexOfDefaultTexture = textureAttributes[i].index;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(kc_PaintOnAlphamap))
        {
            PaintOnAlphamap();
        }
    }

    void PaintOnAlphamap()
    {
        for (int y = 0; y < data.alphamapHeight; y++)
        {
            for (int x = 0; x < data.alphamapWidth; x++)
            {
                float x_01 = (float)x / (float)data.alphamapWidth;
                float y_01 = (float)y / (float)data.alphamapHeight;

                float height = data.GetHeight(Mathf.RoundToInt(y_01 * data.heightmapHeight), Mathf.RoundToInt(x_01 * data.heightmapWidth));
                // Wir normalisieren auf die tatsächliche Höhe, statt höchstmögliche Höhe, um nicht bei Höhenänderungen neu einstellen zu müssen.
                float normHeight = height / maxHeight;
                // steepness liefert einen Winkel zwischen 0 und 90 als Wert zurück.
                float steepness = data.GetSteepness(y_01, x_01);
                float normSteepness = steepness / 90f;
                // Clear all
                for (int i = 0; i < data.alphamapLayers; i++)
                {
                    alphamap[x, y, i] = 0.0f;
                }

                float[] weights = new float[data.alphamapLayers];

                // durch alle Textures steppen und, falls innerhalb der gewünschten Range, den jeweiligen alphamap-Wert auf 1 setzen
                for (int i = 0; i < textureAttributes.Count; i++)
                {
                    if (normHeight >= textureAttributes[i].minAltitude && normHeight <= textureAttributes[i].maxAltitude && normSteepness >= textureAttributes[i].minSteepness && normSteepness <= textureAttributes[i].maxSteepness)
                    {
                        weights[textureAttributes[i].index] = 1.0f;
                    }
                }

                // Normalisierungsfaktor z, Beispiel: wenn 4 Texturen auf Wert 1 stehen ist dies 4
                float z = weights.Sum();
                // Falls die Summe nahe 0 ist, ist keine Textur gesetzt, dann Default Texture benutzen
                if (Mathf.Approximately(z, 0.0f))
                {
                    weights[indexOfDefaultTexture] = 1.0f;
                }

                // durch alle Layer steppen, Wert normalisieren(Beispiel 1/4 = 0.25) und dem alphamap-Array einschreiben.
                for (int i = 0; i < data.alphamapLayers; i++)
                {
                    weights[i] /= z;
                    alphamap[x, y, i] = weights[i];
                }
            }
        }
        // nachdem das für alle Koordinaten geschehen ist, Array der TerrainData übergeben, startend bei (0,0)
        data.SetAlphamaps(0, 0, alphamap);
    }

    float GetMaxHeight(TerrainData data, int heightmapWidth)
    {
        float max = 0f;
        for (int y = 0; y < heightmapWidth; y++)
        {
            for (int x = 0; x < heightmapWidth; x++)
            {
                if (data.GetHeight(y, x) > max)
                {
                    max = data.GetHeight(y, x);
                }
            }
        }
        return max;
    }
}

// Datenklasse für die Textureigenschaften
[Serializable]
public class TextureData
{
    public string name;
    public int index;
    public bool defaultTexture = false;
    [Range(0.0f, 1.0f)] public float minSteepness;
    [Range(0.0f, 1.0f)] public float maxSteepness;
    [Range(0.0f, 1.0f)] public float minAltitude;
    [Range(0.0f, 1.0f)] public float maxAltitude;
}
