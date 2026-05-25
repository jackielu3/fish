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

    private float timer = 0f;
    [SerializeField] private float secsBetweenSpawn = 3f;

    private void Start()
    {
        CreateSpawnBoundaries();

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
        Vector2 halfSize = spawnAreaSize * 0.5f;

        Vector2 spawnPos = new(
            Random.Range(spawnAreaCenter.x - halfSize.x, spawnAreaCenter.x + halfSize.x),
            Random.Range(spawnAreaCenter.y - halfSize.y, spawnAreaCenter.y + halfSize.y)
        );

        Fish fish = Instantiate(fishType.prefab, spawnPos, Quaternion.identity).GetComponent<Fish>(); ;
        fish.Initialize(fishType, this);

        timer = 0f;
        numSpawned++;
    }

    private void CreateSpawnBoundaries()
    {
        Vector2 halfSize = spawnAreaSize * 0.5f;
        float thickness = 0.25f;

        CreateBoundary(
            "Top Boundary",
            spawnAreaCenter + Vector3.up * halfSize.y,
            new Vector2(spawnAreaSize.x, thickness),
            Vector2.down
        );

        CreateBoundary(
            "Bottom Boundary",
            spawnAreaCenter + Vector3.down * halfSize.y,
            new Vector2(spawnAreaSize.x, thickness),
            Vector2.up
        );

        CreateBoundary(
            "Left Boundary",
            spawnAreaCenter + Vector3.left * halfSize.x,
            new Vector2(thickness, spawnAreaSize.y),
            Vector2.right
        );

        CreateBoundary(
            "Right Boundary",
            spawnAreaCenter + Vector3.right * halfSize.x,
            new Vector2(thickness, spawnAreaSize.y),
            Vector2.left
        );
    }

    private void CreateBoundary(string boundaryName, Vector3 position, Vector2 size, Vector2 bounceNormal)
    {
        GameObject boundaryObject = new GameObject(boundaryName);
        boundaryObject.transform.SetParent(transform);
        boundaryObject.transform.position = position;

        BoxCollider2D collider = boundaryObject.AddComponent<BoxCollider2D>();
        collider.size = size;
        collider.isTrigger = true;

        FishSpawnBoundary boundary = boundaryObject.AddComponent<FishSpawnBoundary>();
        boundary.Initialize(this, bounceNormal);
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
