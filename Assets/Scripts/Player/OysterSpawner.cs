#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

public class OysterSpawner : MonoBehaviour
{
    [SerializeField] private Oyster oysterPrefab;
    [SerializeField] private int maxSpawned = 10;
    [SerializeField] private Vector3 spawnAreaCenter;
    [SerializeField] private Vector2 spawnAreaSize = new(5f, 1f);

    [SerializeField] private bool isSpawnerActive = false;
    [SerializeField] private LayerMask bigRockLayer;
    [SerializeField] private float spawnCheckRadius = 0.4f;
    [SerializeField] private int maxSpawnAttempts = 30;

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private Vector2 gizmoSpriteSize = Vector2.one;
    [SerializeField] private Vector3 labelOffset = Vector3.up * 1.5f;

    private readonly List<Oyster> spawnedOysters = new();

    private void Start()
    {
        for (int i = 0; i < maxSpawned; i++)
        {
            SpawnOyster();
        }

        SetSpawnerActive(isSpawnerActive);
    }

    private void SpawnOyster()
    {
        if (oysterPrefab == null) return;
        if (!TryGetSpawnPosition(out Vector2 spawnPos)) return;

        Oyster oyster = Instantiate(oysterPrefab, spawnPos, Quaternion.identity);
        spawnedOysters.Add(oyster);
    }

    public void SetSpawnerActive(bool active)
    {
        isSpawnerActive = active;

        for (int i = spawnedOysters.Count - 1; i >= 0; i--)
        {
            if (spawnedOysters[i] == null)
            {
                spawnedOysters.RemoveAt(i);
                continue;
            }

            spawnedOysters[i].Close();
            spawnedOysters[i].gameObject.SetActive(active);
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
                spawnCheckRadius,
                bigRockLayer
            );

            if (!insideBigRock)
                return true;
        }

        spawnPos = Vector2.zero;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);

#if UNITY_EDITOR
        Handles.Label(
            spawnAreaCenter + labelOffset,
            $"{maxSpawned}"
        );

        if (oysterPrefab != null)
        {
            Vector3 worldPos = spawnAreaCenter;
            Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPos);

            float size = 48f;

            Handles.BeginGUI();

            GUI.Box(
                new Rect(
                    guiPos.x - gizmoSpriteSize.x / 2f,
                    guiPos.y - gizmoSpriteSize.y / 2f,
                    size,
                    size
                ),
                "Oysters"
            );

            Handles.EndGUI();
        }
#endif
    }
}
