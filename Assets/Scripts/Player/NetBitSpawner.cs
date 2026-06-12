using System.Collections.Generic;
using UnityEngine;

public class NetBitSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private NetBitPickup netBitPrefab;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaCenter;
    [SerializeField] private Vector2 spawnAreaSize = new(10f, 10f);

    [Header("Spawn Settings")]
    [SerializeField] private int maxSpawned = 5;
    [SerializeField] private float secondsBetweenSpawn = 8f;
    [SerializeField] private LayerMask blockedLayers;
    [SerializeField] private float spawnCheckRadius = 0.3f;
    [SerializeField] private int maxSpawnAttempts = 20;

    private readonly List<NetBitPickup> spawnedBits = new();
    private float timer;

    private bool IsUnlocked => upgradeManager.GetUpgradeValue(UpgradeType.NetFragments) > 0f;

    private void Update()
    {
        Debug.Log(upgradeManager.GetUpgradeValue(UpgradeType.NetFragments) > 0f);
        if (!IsUnlocked)
        {
            ClearAllBits();
            return;
        }

        CleanupNullBits();

        if (spawnedBits.Count >= maxSpawned)
            return;

        timer += Time.deltaTime;

        if (timer >= secondsBetweenSpawn)
        {
            SpawnBit();
            timer = 0f;
        }
    }

    private void SpawnBit()
    {
        Debug.Log("spawn bit");
        if (netBitPrefab == null) return;
        Debug.Log("spawn bit 2");
        if (!TryGetSpawnPosition(out Vector2 spawnPos)) return;

        Debug.Log("spawn bit 3");
        NetBitPickup bit = Instantiate(netBitPrefab, spawnPos, Quaternion.identity);
        spawnedBits.Add(bit);
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

            bool blocked = Physics2D.OverlapCircle(
                spawnPos,
                spawnCheckRadius,
                blockedLayers
            );

            if (!blocked)
                return true;
        }

        spawnPos = Vector2.zero;
        return false;
    }

    private void CleanupNullBits()
    {
        for (int i = spawnedBits.Count - 1; i >= 0; i--)
        {
            if (spawnedBits[i] == null)
                spawnedBits.RemoveAt(i);
        }
    }

    private void ClearAllBits()
    {
        for (int i = spawnedBits.Count - 1; i >= 0; i--)
        {
            if (spawnedBits[i] != null)
                Destroy(spawnedBits[i].gameObject);
        }

        spawnedBits.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}