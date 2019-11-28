using UnityEngine;

public class RandomSeed : MonoBehaviour
{
    const int numSeeds = 2;
    [SerializeField] KeyCode kc_SetRandomInitState = KeyCode.Space;
    [SerializeField] bool generateRandomSeeds = true;
    [SerializeField] int[] seeds = new int[numSeeds];
    [SerializeField, Range(0, numSeeds - 1)] int useSeedIdx;
    [SerializeField] GUIStyle style;
    int randVal;

    void Start()
    {
        if (generateRandomSeeds)
        {
            GenerateSeeds();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(kc_SetRandomInitState))
        {
            SetRandomInitState(useSeedIdx);
        }
    }

    void GenerateSeeds()
    {
        for (int i = 0; i < seeds.Length; i++)
        {
            seeds[i] = Random.Range(0, 100);
        }
    }

    void SetRandomInitState(int idx)
    {
        Random.InitState(seeds[idx]);
        randVal = Random.Range(0, 800);
    }

    void OnGUI()
    {
        GUILayout.Label(randVal.ToString(), style);
    }
}
