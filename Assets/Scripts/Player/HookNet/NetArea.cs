using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class NetArea : MonoBehaviour
{
    private PolygonCollider2D polygonCollider;
    [SerializeField] private LayerMask fishLayer;

    public void Awake()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();
        polygonCollider.isTrigger = false;
    }

    public void Initialize(List<Vector2> points)
    {
        if (points == null || points.Count < 3) return;

        List<Vector3> worldPoints = new();

        for (int i = 0; i < points.Count; i++)
        {
            worldPoints.Add(points[i]);
        }

        UpdateShape(worldPoints);
        CatchFish();
    }

    private Vector2 GetCenter(List<Vector2> points)
    {
        Vector2 total = Vector2.zero;

        for (int i = 0; i < points.Count; i++)
        {
            total += points[i];
        }

        return total / points.Count;
    }

    private Vector2 GetCenter(List<Vector3> points)
    {
        Vector2 total = Vector2.zero;

        for (int i = 0; i < points.Count; i++)
        {
            total += (Vector2)points[i];
        }

        return total / points.Count;
    }

    private void CatchFish()
    {
        ContactFilter2D filter = new();
        filter.SetLayerMask(fishLayer);
        filter.useTriggers = true;

        List<Collider2D> results = new();
        polygonCollider.Overlap(filter, results);

        foreach (Collider2D result in results)
        {
            if (!result.TryGetComponent<Fish>(out var fish)) continue;
            fish.Catch();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Net area triggered with: " + collision.gameObject.name);
    }

    public void UpdateShape(List<Vector3> worldPoints)
    {
        if (worldPoints == null || worldPoints.Count < 3) return;

        Vector2 center = GetCenter(worldPoints);
        transform.position = center;

        Vector2[] localPoints = new Vector2[worldPoints.Count];

        for (int i = 0; i < worldPoints.Count; i++)
        {
            localPoints[i] = transform.InverseTransformPoint(worldPoints[i]);
        }

        polygonCollider.enabled = true;
        polygonCollider.pathCount = 1;
        polygonCollider.SetPath(0, localPoints);
    }
}
