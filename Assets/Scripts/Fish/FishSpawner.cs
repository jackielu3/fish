#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FishData fishType;

    [Header("Values")]
    [SerializeField] private int maxSpawned;
    [SerializeField][ReadOnly] private int numSpawned;
    [SerializeField] private Vector3 spawnAreaCenter;
    [SerializeField] private Vector2 spawnAreaSize = new(5f, 3f);

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private Vector2 gizmoSpriteSize = Vector2.one;
    [SerializeField] private Vector3 labelOffset = Vector3.up * 1.5f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask bigRockLayer;
    [SerializeField] private float fishSpawnCheckRadius = 0.4f;
    [SerializeField] private int maxSpawnAttempts = 30;

    [Header("Spawner State")]
    [SerializeField] private bool isSpawnerActive = true;
    [SerializeField] private bool spawnAllOnStart = true;

    private readonly List<Fish> spawnedFish = new();

    private float timer = 0f;
    [SerializeField] private float secsBetweenSpawn = 3f;

    private int baseMaxSpawned;

    private void Awake()
    {
        baseMaxSpawned = maxSpawned;
    }

    private void Start()
    {
        if (!spawnAllOnStart) return;

        for (int i = 0; i < maxSpawned; i++)
        {
            SpawnFish();
        }

        SetSpawnerActive(isSpawnerActive);
    }

    private void Update()
    {
        if (!isSpawnerActive) return;
        if (numSpawned >= maxSpawned) return;

        timer += Time.deltaTime;

        if (timer >= secsBetweenSpawn)
        {
            SpawnFish();
        }
    }

    public void SetMaxSpawnedBonus(int bonus)
    {
        maxSpawned = baseMaxSpawned + bonus;
    }

    private void SpawnFish()
    {
        if (!TryGetSpawnPosition(out Vector2 spawnPos))
            return;

        Fish fish = Instantiate(fishType.prefab, spawnPos, Quaternion.identity).GetComponent<Fish>();
        fish.Initialize(fishType, this);
        spawnedFish.Add(fish);
        fish.gameObject.SetActive(isSpawnerActive);

        timer = 0f;
        numSpawned++;
    }

    public void SetSpawnerActive(bool active)
    {
        isSpawnerActive = active;

        for (int i = spawnedFish.Count - 1; i >= 0; i--)
        {
            if (spawnedFish[i] == null)
            {
                spawnedFish.RemoveAt(i);
                continue;
            }

            spawnedFish[i].gameObject.SetActive(isSpawnerActive);
        }
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

            bool insideBigRock = Physics2D.OverlapCircle(
                spawnPos,
                fishSpawnCheckRadius,
                bigRockLayer
            );

            if (!insideBigRock)
                return true;
        }

        spawnPos = Vector2.zero;
        return false;
    }

    public Bounds GetSpawnBounds()
    {
        return new Bounds(spawnAreaCenter, spawnAreaSize);
    }

    public void CaughtFish(Fish fish = null)
    {
        numSpawned -= 1;

        if (fish != null)
            spawnedFish.Remove(fish);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);

#if UNITY_EDITOR
        Handles.Label(
            spawnAreaCenter + labelOffset,
            $"{numSpawned}/{maxSpawned}"
        );

        if (fishType != null && fishType.sprite != null)
        {
            Texture2D preview = fishType.sprite.texture;

            if (preview != null)
            {
                Vector3 worldPos = spawnAreaCenter;
                Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPos);

                float size = 48f;

                Handles.BeginGUI();

                GUI.DrawTexture(
                    new Rect(
                        guiPos.x - gizmoSpriteSize.x / 2f,
                        guiPos.y - gizmoSpriteSize.y / 2f,
                        size,
                        size
                    ),
                    preview
                );

                Handles.EndGUI();
            }
        }
#endif
    }
}
