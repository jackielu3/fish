#if UNITY_EDITOR
using UnityEditor;
#endif
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

    private float timer = 0f;
    [SerializeField] private float secsBetweenSpawn = 3f;

    private void Start()
    {
        for (int i = 0; i < maxSpawned; i++)
        {
            SpawnFish();
        }
    }

    private void Update()
    {
        if (numSpawned >= maxSpawned) return;

        timer += Time.deltaTime;

        if (timer >= secsBetweenSpawn)
        {
            SpawnFish();
        }
    }

private void SpawnFish()
{
    if (!TryGetSpawnPosition(out Vector2 spawnPos))
        return;

    Fish fish = Instantiate(fishType.prefab, spawnPos, Quaternion.identity).GetComponent<Fish>();
    fish.Initialize(fishType, this);

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

    public void CaughtFish()
    {
        numSpawned -= 1;
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
