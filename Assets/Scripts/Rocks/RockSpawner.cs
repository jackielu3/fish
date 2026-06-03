using System.Collections.Generic;
using UnityEngine;

public enum RockSpawnType
{
    BigRock,
    SmallRock
}

public class RockSpawner : MonoBehaviour
{
    [Header("Spawn Type")]
    [SerializeField] private RockSpawnType rockSpawnType;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> rockPrefabs = new();

    [Header("Spawn Area")]
    [SerializeField] private int maxSpawned = 10;
    [SerializeField][ReadOnly] private int numSpawned;
    [SerializeField] private Vector3 spawnAreaCenter;
    [SerializeField] private Vector2 spawnAreaSize = new(5f, 3f);
    [SerializeField] private float secsBetweenSpawn = 3f;

    [Header("Big Rock Spacing")]
    [SerializeField] private float bigRockExclusionRadius = 2f;
    [SerializeField] private LayerMask bigRockLayer;
    [SerializeField] private int maxSpawnAttempts = 30;

    private float timer;

    private void Start()
    {
        for (int i = 0; i < maxSpawned; i++)
        {
            SpawnRock();
        }
    }

    private void Update()
    {
        if (numSpawned >= maxSpawned) return;

        timer += Time.deltaTime;

        if (timer >= secsBetweenSpawn)
        {
            SpawnRock();
        }
    }

    private void SpawnRock()
    {
        if (rockPrefabs.Count == 0) return;

        if (!TryGetSpawnPosition(out Vector2 spawnPos))
            return;

        GameObject prefab = rockPrefabs[Random.Range(0, rockPrefabs.Count)];
        GameObject rock = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (rock.TryGetComponent(out SmallRock smallRock))
        {
            smallRock.Initialize(this);
        }

        timer = 0f;
        numSpawned++;
    }

    private bool TryGetSpawnPosition(out Vector2 spawnPos)
    {
        Vector2 halfSize = spawnAreaSize * 0.5f;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            spawnPos = new Vector2(
                Random.Range(spawnAreaCenter.x - halfSize.x, spawnAreaCenter.x + halfSize.x),
                Random.Range(spawnAreaCenter.y - halfSize.y, spawnAreaCenter.y + halfSize.y)
            );

            if (rockSpawnType == RockSpawnType.BigRock)
            {
                bool tooCloseToBigRock = Physics2D.OverlapCircle(
                    spawnPos,
                    bigRockExclusionRadius,
                    bigRockLayer
                );

                if (tooCloseToBigRock)
                    continue;
            }

            return true;
        }

        spawnPos = Vector2.zero;
        return false;
    }

    public void CaughtRock()
    {
        numSpawned--;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = rockSpawnType == RockSpawnType.BigRock ? Color.gray : Color.yellow;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}