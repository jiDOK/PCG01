using UnityEngine;

public class House : MonoBehaviour
{
    // Die verschiedenen Meshes
    [SerializeField] Mesh[] floorModules;
    [SerializeField] public Mesh floorMesh;
    [SerializeField] public Mesh baseFloorMesh;
    [SerializeField] public Mesh roofMesh;
    // die Hausdaten auch zum einstellen im Editor
    [SerializeField] float baseFloorHeight = 1f;
    [SerializeField] int numberOfFloors = 1;
    [SerializeField] float frontLength = 1f;
    [SerializeField] float sideLength = 1f;
    [SerializeField] float roofHeight = 1f;

    ShapeGrammar houseGrammar;
    int numMeshes;
    Vector3 pos;
    Quaternion rot;
    Vector3 scale;
    public Material mat;
    Matrix4x4 mtrx;
    Transform baseFloor;
    Transform roof;
    Transform frontWall;
    Transform leftWall;
    Transform rightWall;
    Transform backWall;
    bool houseBuilt;
    LayerMask layer;
    int[] randomIndices = new int[32];

    void Start()
    {
        houseGrammar = new ShapeGrammar();
        // Generate wird aufgerufen und die BuildHouse-Methode als Action übergeben
        houseGrammar.Generate(BuildHouse);
        layer = LayerMask.NameToLayer("Default");
        // zufällige Indices für Variation
        for (int i = 0; i < randomIndices.Length; i++)
        {
            randomIndices[i] = Random.Range(0, floorModules.Length);
        }
    }

    void BuildHouse(HouseData data)
    {
        baseFloorHeight = data.BaseFloor.Height;
        roofHeight = data.Roof.Height;
        numberOfFloors = data.Floors.Count;
        frontLength = data.BaseFloor.Width;
        sideLength = data.BaseFloor.Length;
        scale = Vector3.one;
        // GameObjects, damit wir Transforms benutzen können.
        GameObject bF = new GameObject();
        GameObject fW = new GameObject();
        GameObject lW = new GameObject();
        GameObject rW = new GameObject();
        GameObject bW = new GameObject();
        GameObject r = new GameObject();
        // bei den Hilfsobjekten ist es nicht nötig, daß sie in der Hierarchy zu sehen sind
        bF.hideFlags = fW.hideFlags = lW.hideFlags = rW.hideFlags = bW.hideFlags = r.hideFlags = HideFlags.HideInHierarchy;
        // "Ernten" der Transforms
        baseFloor = bF.transform;
        frontWall = fW.transform;
        rightWall = rW.transform;
        leftWall = lW.transform;
        backWall = bW.transform;
        roof = r.transform;
        // Zum Initialisieren werden Positions- und RotationsOffsets mitgegeben.
        InitializeBaseFloor(baseFloor, Vector3.zero);
        InitializeWall(frontWall, new Vector3(0f, baseFloorHeight, 1f * sideLength / 2), 0f);
        InitializeWall(rightWall, new Vector3(-(frontLength / 2), data.BaseFloor.Height, 0f), 270f);
        InitializeWall(leftWall, new Vector3(frontLength / 2, data.BaseFloor.Height, 0f), 90f);
        InitializeWall(backWall, new Vector3(0f, data.BaseFloor.Height, -(sideLength / 2)), 180f);
        float roofYOffset = numberOfFloors + baseFloorHeight;
        InitializeRoof(roof, new Vector3(0f, roofYOffset, 0f));
        houseBuilt = true;
    }

    void InitializeBaseFloor(Transform bFloor, Vector3 posOffset)
    {
        bFloor.parent = transform;
        bFloor.localPosition = posOffset;
    }

    void InitializeWall(Transform wall, Vector3 posOffset, float angle)
    {
        wall.parent = transform;
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
            baseFloor.localScale = new Vector3(frontLength, baseFloorHeight, sideLength);
            DrawBaseFloor(baseFloor);
            // nach dem Erdgeschoß werden nach und nach die einzelnen Stockwerke auf Position gebracht und gerendert
            for (int i = 0; i < numberOfFloors; i++)
            {
                frontWall.localPosition = new Vector3(0f, i + baseFloorHeight, sideLength / 2);
                rightWall.localPosition = new Vector3(-(frontLength / 2), i + baseFloorHeight, 0f);
                leftWall.localPosition = new Vector3(frontLength / 2, i + baseFloorHeight, 0f);
                backWall.localPosition = new Vector3(0f, i + baseFloorHeight, -(sideLength / 2));
                DrawWall(frontLength, frontWall);
                DrawWall(sideLength, rightWall);
                DrawWall(sideLength, leftWall);
                DrawWall(frontLength, backWall);
            }
            // und schließlich das Dach
            roof.localPosition = Vector3.up * (numberOfFloors + baseFloorHeight);
            roof.localScale = new Vector3(frontLength, roofHeight, sideLength);
            DrawRoof(roof);
        }
    }

    void DrawBaseFloor(Transform bFloor)
    {
        Vector3 offset = new Vector3(0f, baseFloorHeight / 2f, 0f);
        Quaternion newRot = transform.rotation;
        Vector3 newPos = transform.position + (newRot * offset);
        mtrx = Matrix4x4.TRS(newPos, newRot, bFloor.localScale);
        Graphics.DrawMesh(baseFloorMesh, mtrx, mat, layer);
    }

    void DrawWall(float wallLength, Transform wall)
    {
        numMeshes = Mathf.FloorToInt(wallLength);
        float rest = wallLength % 1;
        float offset = rest / numMeshes;
        Vector3 newPos;
        Quaternion newRot = transform.rotation * wall.localRotation;
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
        Vector3 offset = Vector3.up * rf.localScale.y * 0.5f;
        // der offset muß mitrotiert werden
        Vector3 newPos = rf.position + (newRot * offset);
        mtrx = Matrix4x4.TRS(newPos, newRot, rf.localScale);
        Graphics.DrawMesh(roofMesh, mtrx, mat, layer);
    }
}
