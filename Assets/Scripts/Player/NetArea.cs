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
        polygonCollider.isTrigger = true;
    }

    public void Initialize(List<Vector2> points)
    {
        Vector2 center = GetCenter(points);
        transform.position = center;

        Vector2[] localPoints = new Vector2[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            localPoints[i] = transform.InverseTransformPoint(points[i]);
        }

        polygonCollider.pathCount = 1;
        polygonCollider.SetPath(0, localPoints);

        CatchFish();
        polygonCollider.enabled = false;
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

    private void CatchFish()
    {
        ContactFilter2D filter = new();
        filter.SetLayerMask(fishLayer);
        filter.useTriggers = true;

        List<Collider2D> results = new();
        polygonCollider.Overlap(filter, results);

        foreach (Collider2D result in results)
        {
            Fish fish = result.GetComponent<Fish>();

            if (fish != null)
            {
                fish.Catch();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Net area triggered with: " + collision.gameObject.name);
    }
}
