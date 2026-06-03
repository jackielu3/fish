using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D), typeof(EdgeCollider2D))]
public class NetArea : MonoBehaviour
{
    private PolygonCollider2D polygonCollider;
    private EdgeCollider2D edgeCollider;

    private static readonly List<Collider2D> overlapResults = new();

    [SerializeField] private LayerMask fishLayer;
    [SerializeField] private float wallThickness = 0.15f;
    [SerializeField] private float insidePadding = 0.25f;

    private readonly List<Vector2> currentWorldPoints = new();
    private readonly List<Fish> caughtFish = new();

    private bool hasCaughtFish = false;

    public void Awake()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();
        edgeCollider = GetComponent<EdgeCollider2D>();

        polygonCollider.isTrigger = true;

        edgeCollider.isTrigger = false;
        edgeCollider.edgeRadius = wallThickness;
    }

    public void Initialize(List<Vector2> points)
    {
        if (points == null || points.Count < 3) return;

        List<Vector3> worldPoints = new();

        foreach (Vector2 point in points)
        {
            worldPoints.Add(point);
        }

        UpdateShape(worldPoints);
    }

    public void UpdateShape(List<Vector3> worldPoints)
    {
        if (worldPoints == null || worldPoints.Count < 3) return;

        currentWorldPoints.Clear();

        foreach (Vector3 point in worldPoints)
        {
            currentWorldPoints.Add(point);
        }
        Vector2 center = GetCenter(currentWorldPoints);
        transform.position = center;

        Vector2[] localPolygonPoints = new Vector2[currentWorldPoints.Count];

        for (int i = 0; i < currentWorldPoints.Count; i++)
        {
            localPolygonPoints[i] = transform.InverseTransformPoint(currentWorldPoints[i]);
        }

        polygonCollider.pathCount = 1;
        polygonCollider.SetPath(0, localPolygonPoints);

        List<Vector2> localEdgePoints = new();

        for (int i = 0; i < currentWorldPoints.Count; i++)
        {
            localEdgePoints.Add(transform.InverseTransformPoint(currentWorldPoints[i]));
        }

        localEdgePoints.Add(localEdgePoints[0]);

        edgeCollider.SetPoints(localEdgePoints);
    }

    public CatchResult CatchFish()
    {
        CatchResult catchResult = new();

        if (hasCaughtFish) return catchResult;
        hasCaughtFish = true;

        ContactFilter2D filter = new();
        filter.SetLayerMask(fishLayer);
        filter.useTriggers = true;

        overlapResults.Clear();
        polygonCollider.Overlap(filter, overlapResults);

        foreach (Collider2D result in overlapResults)
        {
            if (result.TryGetComponent<Fish>(out Fish fish))
            {
                if (caughtFish.Contains(fish)) continue;

                caughtFish.Add(fish);
                fish.SetContainingNet(this, insidePadding);
                fish.Catch();

                catchResult.AddFish(fish);
                continue;
            }

            if (result.TryGetComponent<SmallRock>(out SmallRock smallRock))
            {
                smallRock.Catch();
                catchResult.AddSmallRock(smallRock);

                if (smallRock.owningSpawner != null)
                    smallRock.owningSpawner.CaughtRock();

                Destroy(smallRock.gameObject);
            }
        }

        return catchResult;
    }

    public void DestroyCaughtFish()
    {
        foreach (Fish fish in caughtFish)
        {
            if (fish == null) continue;

            if (fish.owningSpawner != null)
            {
                fish.owningSpawner.CaughtFish();
            }

            Destroy(fish.gameObject);
        }

        caughtFish.Clear();
    }

    public bool TryGetInsideCorrection(Vector2 position, float padding, out Vector2 correctedPosition, out Vector2 inwardNormal)
    {
        correctedPosition = position;
        inwardNormal = Vector2.zero;

        if (currentWorldPoints.Count < 3) return false;
        if (ContainsPoint(position)) return false;

        Vector2 center = GetCenter(currentWorldPoints);

        Vector2 closest = currentWorldPoints[0];
        float closestSqrDistance = float.MaxValue;

        for (int i = 0; i < currentWorldPoints.Count; i++)
        {
            Vector2 a = currentWorldPoints[i];
            Vector2 b = currentWorldPoints[(i + 1) % currentWorldPoints.Count];

            Vector2 pointOnSegment = ClosestPointOnSegment(position, a, b);
            float sqrDistance = (position - pointOnSegment).sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closest = pointOnSegment;
            }
        }

        inwardNormal = (center - closest).normalized;

        if (inwardNormal.sqrMagnitude < 0.001f)
        {
            inwardNormal = (center - position).normalized;
        }

        correctedPosition = closest + inwardNormal * padding;
        return true;
    }

    private bool ContainsPoint(Vector2 point)
    {
        bool inside = false;

        for (int i = 0, j = currentWorldPoints.Count - 1; i < currentWorldPoints.Count; j = i++)
        {
            Vector2 pi = currentWorldPoints[i];
            Vector2 pj = currentWorldPoints[j];

            bool intersects =
                ((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x);

            if (intersects)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);

        return a + ab * t;
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
}
