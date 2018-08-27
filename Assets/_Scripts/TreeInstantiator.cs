using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeInstantiator : MonoBehaviour
{
    [SerializeField] KeyCode kc_SpawnTrees = KeyCode.T;
    [SerializeField] Texture2D heightTex;
    [SerializeField] TreeData[] trees;
    [SerializeField] float treeMinHeight = 0f;
    [SerializeField] float treeMaxHeight = 1f;
    [SerializeField, Range(0.1f, 10f)] float treeScale = 1f;
    [SerializeField] bool spawnedOnce;
    //consts, um zusammenhängende Arrays auf den selben Wert initialisieren zu können
    const int numPositions = 512;
    const int numRotations = 16;
    const int numScalesColors = 32;
    MaterialPropertyBlock block;
    LayerMask layer;
    Matrix4x4 mtrx;
    List<Vector3> treePositions = new List<Vector3>(numPositions);
    List<int> treeIndices = new List<int>(numPositions);
    List<Quaternion> treeRotations = new List<Quaternion>(numRotations);
    List<Vector3> treeScales = new List<Vector3>(numScalesColors);
    List<Color> treeColors = new List<Color>(numScalesColors);
    int[,] detailValues;
    int colorID;

    Terrain terrain;
    TerrainData data;
    PoissonDiscSampler sampler;

    void Start()
    {
        // initialisieren
        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;
        sampler = new PoissonDiscSampler(data.size.x, data.size.z, 7f);
        block = new MaterialPropertyBlock();
        // auf Dauer sparsamer es als ID zu benutzen, statt per string
        colorID = Shader.PropertyToID("_Color");
        layer = LayerMask.NameToLayer("Default");
        mtrx = new Matrix4x4();
        // 16 eingeschränkt zufällige Rotationen stehen zur Wahl
        for (int i = 0; i < treeRotations.Capacity; i++)
        {
            treeRotations.Add(Quaternion.Euler(new Vector3(Random.Range(0f, 8f), Random.Range(0f, 360), Random.Range(0f, 8f))));
        }
        // 32 eingeschränkt zufällige Farben stehen zur Wahl
        // ebenso 32 Scales
        for (int i = 0; i < treeColors.Capacity; i++)
        {
            treeColors.Add(Random.ColorHSV(0f, 0.6f, 0f, 0.5f, 0.3f, 0.6f));
            treeScales.Add(Vector3.one * Random.Range(2.2f, 3.9f));
        }
    }

    void Update()
    {
        DrawTrees();
    }

    private void DrawTrees()
    {
        // Code in dieser Methode sollte noch aufgeteilt werden, so dass man z.B. auch respawnen kann
        if (!spawnedOnce && Input.GetKeyDown(kc_SpawnTrees))
        {
            spawnedOnce = true;
            detailValues = new int[data.detailHeight, data.detailWidth];
            // Zufalls-Positionen vom Poisson Disc Sampler
            foreach (Vector2 sample in sampler.Samples())
            {
                // GetHeight wäre möglicherweise performanter..
                float height = terrain.SampleHeight(new Vector3(sample.x, 0f, sample.y));
                // Überprüfen, ob im Range
                if (height > treeMinHeight && height < treeMaxHeight)
                {
                    // Graphics.DrawMesh nutzt später die Positionen
                    treePositions.Add(new Vector3(sample.x, height, sample.y));
                    // zufälliger Index für Baumtypvariation
                    treeIndices.Add(Random.Range(0, trees.Length));
                    // Zusätzliche Bedingung für Gras
                    if (height > treeMinHeight + 1f)
                    {
                        // Zufallsposition in detailLayer-Koordinaten übersetzt
                        int dX = (int)(sample.x / data.size.x * data.detailWidth);
                        int dY = (int)(sample.y / data.size.z * data.detailHeight);
                        // FillPatch setzt Gras um die übersetzten Koordinaten des Baumes herum
                        FillPatch(dY, dX);
                    }
                }
            }
            // am Ende die Daten übergeben (wenn es mehr Layer werden lohnt sich bald eine for-Schleife)
            data.SetDetailLayer(0, 0, 0, detailValues);
            data.SetDetailLayer(0, 0, 1, detailValues);
        }
        int rotIdx = 0;
        int colorIdx = 0;
        for (int i = 0; i < treePositions.Count; i++)
        {
            // wenn keine Rotationen/Farben mehr übrig sind, fängt es bei 0 wieder an
            rotIdx = i % treeRotations.Count;
            colorIdx = i % treeColors.Count;
            // Matrix füllen
            mtrx = Matrix4x4.TRS(treePositions[i], treeRotations[rotIdx], treeScales[colorIdx] * treeScale);
            // Die Bäume haben speziellen Shader, der pro Instanz Farbvariationen zuläßt
            block.SetColor(colorID, treeColors[colorIdx]);
            // wir übergeben mesh, matrix, material, layer, camera(default-Wert null),  submeshindex(default 0), 
            // außerdem den manipulierten propertyblock, und die Anweisung, Schatten zu werfen
            Graphics.DrawMesh(trees[treeIndices[i]].trunkMesh, mtrx, trees[treeIndices[i]].trunkMat, layer, null, 0, block, true);
            Graphics.DrawMesh(trees[treeIndices[i]].leafMesh, mtrx, trees[treeIndices[i]].leafMat, layer, null, 0, block, true);
        }
    }

    void FillPatch(int y, int x)
    {
        // Wir loopen um x und y herum
        for (int i = y - 4; i < y + 4; i++)
        {
            for (int j = x - 4; j < x + 4; j++)
            {
                if (i < 0 || j < 0 || i > detailValues.GetLength(0) - 1 || j > detailValues.GetLength(1) - 1)
                {
                    continue;
                }
                else
                {
                    detailValues[i, j] = 1;
                }
            }
        }
    }
}

[Serializable]
public class TreeData
{
    public Mesh trunkMesh;
    public Mesh leafMesh;
    public Material trunkMat;
    public Material leafMat;
}