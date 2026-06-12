#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BaitObject : MonoBehaviour
{
    [System.Serializable]
    public class BaitFishSpawnOption
    {
        public FishData fishData;
        public float minY;
        public float maxY;
        public float fishWeight = 1f;
    }

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private float totalFishWeight = 10f;
    [SerializeField] private List<BaitFishSpawnOption> fishOptions = new();

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private Vector3 labelOffset = Vector3.up * 1.5f;

    [Header("Visual Chunks")]
    [SerializeField] private Transform[] chunks;
    [SerializeField] private float chunkSpreadRadius = 0.5f;
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float initialScatterSpeed = 2f;
    [SerializeField] private float scatterDeceleration = 8f;
    [SerializeField] private float initialSpinSpeed = 720f;
    [SerializeField] private float spinDeceleration = 2500f;

    [Header("Oysters")]
    [SerializeField] private LayerMask oysterLayer;

    private bool isScattering;
    private Vector3[] idleLocalPositions;

    private readonly List<Fish> spawnedFish = new();
    private Vector3[] chunkStartPositions;

    private bool hasSpawnedFish;
    private bool shouldDestroyOnNextBoatReturn;

    private void Awake()
    {
        SetupChunks();
        StartCoroutine(ScatterChunksRoutine());
    }

    private void Update()
    {
        AnimateChunks();
    }

    private void SetupChunks()
    {
        if (chunks == null || chunks.Length == 0) return;

        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i] == null) continue;

            Vector2 randomOffset = Random.insideUnitCircle * chunkSpreadRadius;
            chunks[i].localPosition = randomOffset;
        }
    }

    private void AnimateChunks()
    {
        if (isScattering) return;
        if (chunks == null) return;

        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i] == null) continue;

            Vector3 localPos = idleLocalPositions[i];
            localPos.y += Mathf.Sin(Time.time * floatSpeed + i) * floatAmplitude;
            chunks[i].localPosition = localPos;
        }
    }

    public void OnReturnedToBoat()
    {

        if (!hasSpawnedFish)
        {
            OpenNearbyOysters();
            SpawnBonusFish();
            hasSpawnedFish = true;
            shouldDestroyOnNextBoatReturn = true;
            return;
        }

        if (shouldDestroyOnNextBoatReturn)
        {
            Debug.Log("Bait despawning bonus fish");
            DestroySpawnedFish();
            Destroy(gameObject);
        }
    }

    private void OpenNearbyOysters()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            spawnRadius,
            oysterLayer
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<Oyster>(out Oyster oyster))
            {
                oyster.TryOpen();
            }
        }
    }

    private IEnumerator ScatterChunksRoutine()
    {
        isScattering = true;
        idleLocalPositions = new Vector3[chunks.Length];

        Vector2[] directions = new Vector2[chunks.Length];
        float[] movementSpeeds = new float[chunks.Length];
        float[] spinSpeeds = new float[chunks.Length];

        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i] == null) continue;

            directions[i] = Random.insideUnitCircle.normalized;

            if (directions[i] == Vector2.zero)
                directions[i] = Vector2.right;

            movementSpeeds[i] = initialScatterSpeed;

            spinSpeeds[i] =
                Random.value > 0.5f
                    ? initialSpinSpeed
                    : -initialSpinSpeed;
        }

        bool anyMoving = true;

        while (anyMoving)
        {
            anyMoving = false;

            for (int i = 0; i < chunks.Length; i++)
            {
                if (chunks[i] == null) continue;

                if (movementSpeeds[i] > 0f)
                {
                    chunks[i].localPosition +=
                        (Vector3)(directions[i] * movementSpeeds[i] * Time.deltaTime);

                    movementSpeeds[i] = Mathf.Max(
                        0f,
                        movementSpeeds[i] - scatterDeceleration * Time.deltaTime
                    );

                    anyMoving = true;
                }

                if (Mathf.Abs(spinSpeeds[i]) > 0f)
                {
                    chunks[i].Rotate(
                        0f,
                        0f,
                        spinSpeeds[i] * Time.deltaTime
                    );

                    spinSpeeds[i] = Mathf.MoveTowards(
                        spinSpeeds[i],
                        0f,
                        spinDeceleration * Time.deltaTime
                    );

                    anyMoving = true;
                }
            }

            yield return null;
        }

        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i] == null) continue;

            idleLocalPositions[i] = chunks[i].localPosition;
        }

        isScattering = false;
    }
    private void SpawnBonusFish()
    {
        float remainingWeight = totalFishWeight;
        int failedAttemptsInRow = 0;
        int maxFailedAttemptsInRow = 5;

        while (remainingWeight > 0f && failedAttemptsInRow < maxFailedAttemptsInRow)
        {
            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
            BaitFishSpawnOption option = GetFishOptionForY(spawnPos.y);

            if (option == null || option.fishData == null || option.fishData.prefab == null)
            {
                failedAttemptsInRow++;
                continue;
            }

            Fish fish = Instantiate(option.fishData.prefab, spawnPos, Quaternion.identity);
            fish.Initialize(option.fishData, null);

            spawnedFish.Add(fish);
            remainingWeight -= option.fishWeight;

            failedAttemptsInRow = 0;
        }
    }

    private BaitFishSpawnOption GetFishOptionForY(float y)
    {
        List<BaitFishSpawnOption> validOptions = new();

        foreach (BaitFishSpawnOption option in fishOptions)
        {
            if (y >= option.minY && y <= option.maxY)
                validOptions.Add(option);
        }

        if (validOptions.Count == 0)
            return null;

        return validOptions[Random.Range(0, validOptions.Count)];
    }

    private void DestroySpawnedFish()
    {
        foreach (Fish fish in spawnedFish)
        {
            if (fish != null)
                Destroy(fish.gameObject);
        }

        spawnedFish.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

#if UNITY_EDITOR
        Handles.Label(
            transform.position + labelOffset,
            $"Bait Radius: {spawnRadius}\nWeight: {totalFishWeight}"
        );
#endif
    }
}