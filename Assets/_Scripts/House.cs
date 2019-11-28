using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    // Die verschiedenen Meshes
    [SerializeField] Mesh[] floorModules;
    [SerializeField] Mesh groundFloorMesh;
    [SerializeField] Mesh roofMesh;
    [SerializeField] Mesh topCapMesh;
    [SerializeField] Mesh bottomCapMesh;
    // die Hausdaten auch zum einstellen im Editor
    [SerializeField, Range(1, 100)] int numberOfFloors = 1;
    [SerializeField, Range(1f, 100f)] float frontLength = 1f;
    [SerializeField, Range(1f, 100f)] float sideLength = 1f;
    [SerializeField, Range(0.1f, 5f)] float groundFloorHeight = 1f;
    [SerializeField, Range(0.1f, 5f)] float roofHeight = 1f;
    [SerializeField, Range(0.1f, 5f)] float maxPlanarOffset = 1f;
    [SerializeField, Range(0f, 360f)] float maxAngleOffset = 0f;
    [SerializeField] AnimationCurve planarOffsetCurve;
    [SerializeField] List<Vector2> planarFloorOffsets = new List<Vector2>(32);
    [SerializeField] List<float> angleOffsets = new List<float>(32);
    [SerializeField] List<float> floorHeights = new List<float>(32);
    [SerializeField] bool useVariation;
    [SerializeField] int maxNumberOfFloors = 4;

    ShapeGrammar houseGrammar;
    int numMeshes;
    int numFloorsAtStart;
    Vector3 pos;
    Quaternion rot;
    Vector3 scale;
    public Material mat;
    Matrix4x4 mtrx;
    Transform floorParent;
    Transform groundFloor;
    Transform roof;
    Transform frontWall;
    Transform leftWall;
    Transform rightWall;
    Transform backWall;
    Transform topCap;
    Transform bottomCap;
    bool houseBuilt;
    LayerMask layer;
    int[] randomIndices = new int[32];

    void Start()
    {
        houseGrammar = new ShapeGrammar(new bool[] { useVariation, useVariation, false }, 3f, 20f, maxPlanarOffset, maxAngleOffset, maxNumberOfFloors);
        // Generate wird aufgerufen und die BuildHouse-Methode als Action übergeben
        houseGrammar.Generate(BuildHouse);
        layer = LayerMask.NameToLayer("Default");
        // zufällige Indices für Variation
        for (int i = 0; i < randomIndices.Length; i++)
        {
            randomIndices[i] = Random.Range(0, floorModules.Length);
        }
        numFloorsAtStart = numberOfFloors;
    }

    void BuildHouse(HouseData houseData)
    {
        groundFloorHeight = houseData.BaseFloor.Data.Height;
        roofHeight = houseData.Roof.Data.Height;
        numberOfFloors = houseData.Floors.Count;
        for (int i = 0; i < houseData.Floors.Count; i++)
        {
            planarFloorOffsets.Add(houseData.Floors[i].Data.PlanarOffset);
            angleOffsets.Add(houseData.Floors[i].Data.AngleOffset);
            floorHeights.Add(houseData.Floors[i].Data.Height);
        }
        frontLength = houseData.BaseFloor.Data.Width;
        sideLength = houseData.BaseFloor.Data.Length;
        scale = Vector3.one;
        // GameObjects, damit wir Transforms benutzen können.
        GameObject fP = new GameObject();
        GameObject gF = new GameObject();
        GameObject fW = new GameObject();
        GameObject lW = new GameObject();
        GameObject rW = new GameObject();
        GameObject bW = new GameObject();
        GameObject r = new GameObject();
        GameObject tC = new GameObject();
        GameObject bC = new GameObject();
        fP.hideFlags = gF.hideFlags = fW.hideFlags = lW.hideFlags = rW.hideFlags = bW.hideFlags = r.hideFlags = tC.hideFlags = bC.hideFlags = HideFlags.HideInHierarchy;
        floorParent = fP.transform;
        groundFloor = gF.transform;
        frontWall = fW.transform;
        rightWall = rW.transform;
        leftWall = lW.transform;
        backWall = bW.transform;
        roof = r.transform;
        topCap = tC.transform;
        bottomCap = bC.transform;

        // Zum Initialisieren werden Positions- und RotationsOffsets mitgegeben.
        InitializeBaseFloor(groundFloor, Vector3.zero);
        InitializeFloorParent(floorParent, Vector3.one * houseData.BaseFloor.Data.Height);
        InitializeWall(frontWall, new Vector3(0f, 0f, 1f * sideLength / 2), 0f);
        InitializeWall(rightWall, new Vector3(-(frontLength / 2), 0f, 0f), 270f);
        InitializeWall(leftWall, new Vector3(frontLength / 2, 0f, 0f), 90f);
        InitializeWall(backWall, new Vector3(0f, 0f, -(sideLength / 2)), 180f);
        InitializeCap(topCap, Vector3.zero);
        InitializeCap(bottomCap, Vector3.zero);
        float roofYOffset = numberOfFloors + houseData.BaseFloor.Data.Height;
        InitializeRoof(roof, new Vector3(0f, roofYOffset, 0f));
        houseBuilt = true;
    }

    void InitializeBaseFloor(Transform bFloor, Vector3 posOffset)
    {
        bFloor.parent = transform;
        bFloor.localPosition = posOffset;
    }

    void InitializeFloorParent(Transform fParent, Vector3 posOffset)
    {
        fParent.parent = transform;
        fParent.localPosition = posOffset;
    }

    void InitializeCap(Transform cap, Vector3 posOffset)
    {
        cap.parent = floorParent;
        cap.localPosition = posOffset;
    }

    void InitializeWall(Transform wall, Vector3 posOffset, float angle)
    {
        wall.parent = floorParent;
        wall.localPosition = posOffset;
        wall.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    void InitializeRoof(Transform roof, Vector3 posOffset)
    {
        roof.parent = transform;
        roof.localPosition = posOffset;
    }

    void Update()
    {
        if (houseBuilt)
        {
            groundFloor.localScale = new Vector3(frontLength, groundFloorHeight, sideLength);
            DrawGroundFloor(groundFloor);
            // nach dem Erdgeschoß werden nach und nach die einzelnen Stockwerke auf Position gebracht und gerendert
            for (int i = 0; i < numberOfFloors; i++)
            {
                DrawFloor(i);
            }
            // und schließlich das Dach
            roof.localPosition = Vector3.up * (numberOfFloors + groundFloorHeight);
            roof.localScale = new Vector3(frontLength, roofHeight, sideLength);
            DrawRoof(roof);
        }
    }

    void DrawGroundFloor(Transform groundFloor)
    {
        Vector3 offset = new Vector3(0f, groundFloorHeight / 2f, 0f);
        Quaternion newRot = transform.rotation;
        Vector3 newPos = transform.position + (newRot * offset);
        mtrx = Matrix4x4.TRS(newPos, newRot, groundFloor.localScale);
        Graphics.DrawMesh(groundFloorMesh, mtrx, mat, layer);
    }

    void DrawFloor(int i)
    {
        var wrappedI = i % numFloorsAtStart;
        float currentHeight = i + groundFloorHeight;
        float curAngle = angleOffsets[wrappedI];
        floorParent.localPosition = new Vector3(0f, currentHeight, 0f);
        floorParent.localRotation = Quaternion.Euler(new Vector3(0f, curAngle, 0f));
        //if (i < planarFloorOffsets.Count && planarFloorOffsets[i] != Vector2.zero)
        float pOC = planarOffsetCurve.Evaluate(i / (float)numberOfFloors);
        if (planarFloorOffsets[wrappedI] != Vector2.zero)
        {
            bottomCap.localPosition = new Vector3(planarFloorOffsets[wrappedI].x * pOC, 0f, planarFloorOffsets[wrappedI].y * pOC);
            //bottomCap.localEulerAngles = new Vector3(bottomCap.localEulerAngles.x, angleOffsets[wrappedI], bottomCap.localEulerAngles.z);
            DrawBottomCap(new Vector3(frontLength, 1f, sideLength), bottomCap);
        }
        //if (i < planarFloorOffsets.Count - 1 && planarFloorOffsets[i + 1] != Vector2.zero)
        if (planarFloorOffsets[wrappedI] != Vector2.zero)
        {
            topCap.localPosition = new Vector3(planarFloorOffsets[wrappedI].x * pOC, 1f, planarFloorOffsets[wrappedI].y * pOC);
            //topCap.localEulerAngles = new Vector3(topCap.localEulerAngles.x, angleOffsets[wrappedI], topCap.localEulerAngles.z);
            DrawTopCap(new Vector3(frontLength, 1f, sideLength), topCap, floorParent);
        }
        //DrawTopCap(new Vector3(0f, currentHeight, 0f) + planarFloorOffsets[i].ToXZ(), new Vector3(frontLength, 1f, sideLength));
        Vector3 offset = planarFloorOffsets[wrappedI].ToXZ() * pOC;
        frontWall.localPosition = new Vector3(0f, 0f, sideLength / 2) + offset;
        rightWall.localPosition = new Vector3(-(frontLength / 2), 0f, 0f) + offset;
        leftWall.localPosition = new Vector3(frontLength / 2, 0f, 0f) + offset;
        backWall.localPosition = new Vector3(0f, 0f, -(sideLength / 2)) + offset;
        DrawWall(frontLength, frontWall, curAngle);
        DrawWall(sideLength, rightWall, curAngle);
        DrawWall(sideLength, leftWall, curAngle);
        DrawWall(frontLength, backWall, curAngle);
    }

    void DrawTopCap(Vector3 scale, Transform tCap, Transform floorParent)
    {
        //Quaternion newRot = transform.rotation * tCap.localRotation;
        Quaternion newRot = floorParent.rotation;
        //Vector3 offset = Vector3.up * tCap.localScale.y * 0.5f;
        //// der offset muß mitrotiert werden
        //Vector3 newPos = tCap.position + (newRot * offset);
        Vector3 newPos = tCap.position;
        mtrx = Matrix4x4.TRS(newPos, newRot, scale);
        Graphics.DrawMesh(topCapMesh, mtrx, mat, layer);
    }

    void DrawBottomCap(Vector3 scale, Transform bCap)
    {
        Quaternion newRot = floorParent.rotation;
        Vector3 newPos = bCap.position;
        mtrx = Matrix4x4.TRS(newPos, newRot, scale);
        Graphics.DrawMesh(bottomCapMesh, mtrx, mat, layer);
    }

    void DrawWall(float wallLength, Transform wall, float angle)
    {
        numMeshes = Mathf.FloorToInt(wallLength);
        float rest = wallLength % 1;
        float offset = rest / numMeshes;
        Vector3 newPos;
        Quaternion newRot = transform.rotation * wall.localRotation * Quaternion.Euler(new Vector3(0f, angle, 0f));
        // die Scale so anpassen, daß immer die ganze Breite gefüllt wird
        Vector3 newScale = new Vector3(scale.x + offset, scale.y, scale.z);
        mtrx = Matrix4x4.TRS(wall.position, Quaternion.identity, Vector3.one);
        for (int i = 0; i < numMeshes; i++)
        {
            // verschieben, so daß die Meshes immer die ganze Breite nach links und rechts ausfüllen
            float j = i - (numMeshes - 1) * 0.5f;
            newPos = wall.position + wall.right * j + wall.right * offset * j;
            // Matrix setzen
            mtrx.SetTRS(newPos, newRot, newScale);
            // Zufallsmesh wird nachgeschlagen, wenn out of Range wieder von 0 anfangen
            int idx = randomIndices[i % randomIndices.Length];
            //Rendern!
            Graphics.DrawMesh(floorModules[idx], mtrx, mat, layer);
        }
    }

    void DrawRoof(Transform rf)
    {
        Quaternion newRot = transform.rotation;
        Vector3 planarOffset = planarFloorOffsets[(numberOfFloors - 1) % numFloorsAtStart].ToXZ() * planarOffsetCurve.Evaluate((numberOfFloors - 1) / (float)numberOfFloors);
        Vector3 offset = (planarOffset + (Vector3.up * rf.localScale.y * 0.5f));
        // der offset muß mitrotiert werden
        Vector3 newPos = rf.position + (newRot * offset);
        mtrx = Matrix4x4.TRS(newPos, newRot, rf.localScale);
        Graphics.DrawMesh(roofMesh, mtrx, mat, layer);
    }
}
