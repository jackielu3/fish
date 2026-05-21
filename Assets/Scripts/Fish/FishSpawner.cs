using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FishData fishType;

    [Header("Values")]
    [SerializeField] private int maxSpawned;
    [SerializeField][ReadOnly] private int numSpawned;
    [SerializeField] private Tuple<Vector2, Vector2> spawnArea;

    private float timer = 0f;
    [SerializeField] private float secsBetweenSpawn = 3f;

    private void Update()
    {
        if (numSpawned >= maxSpawned) return ;

        timer += Time.deltaTime;

        if (timer >= secsBetweenSpawn)
        {
            SpawnFish();
        }
    }

    private void SpawnFish()
    {
        Vector2 spawnPos = new(
            UnityEngine.Random.Range(Mathf.Min(spawnArea.Item1.x, spawnArea.Item2.x), Mathf.Max(spawnArea.Item1.x, spawnArea.Item2.x)),
            UnityEngine.Random.Range(Mathf.Min(spawnArea.Item1.y, spawnArea.Item2.y), Mathf.Max(spawnArea.Item1.y, spawnArea.Item2.y))
        );

        Fish fish = Instantiate(fishType, spawnPos, Quaternion.identity).GetComponent<Fish>(); ;

        fish.transform.position = spawnPos;
        fish.Initialize(fishType);
    }
}
