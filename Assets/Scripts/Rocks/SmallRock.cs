using UnityEngine;

public class SmallRock : MonoBehaviour
{
    public FishData Data => rockCatchData;

    [SerializeField] private FishData rockCatchData;

    public RockSpawner owningSpawner;

    public void Initialize(RockSpawner spawner)
    {
        owningSpawner = spawner;
    }

    public void Catch()
    {
        if (rockCatchData != null)
            rockCatchData.numberCaught++;
    }
}