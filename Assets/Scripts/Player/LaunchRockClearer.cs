using System.Collections.Generic;
using UnityEngine;

public class LaunchRockClearer : MonoBehaviour
{
    [SerializeField] private LayerMask bigRockLayer;
    [SerializeField] private float corridorWidth = 1.5f;

    private readonly List<GameObject> disabledRocks = new();
    private readonly List<Collider2D> results = new();

    private Vector2 debugCenter;
    private Vector2 debugSize;
    private float debugAngle;

    public void ClearRocks(Vector2 start, Vector2 direction, float distance)
    {
        direction.Normalize();

        Vector2 center = start + direction * (distance * 0.5f);
        Vector2 size = new Vector2(corridorWidth, distance);

        float angle = Vector2.SignedAngle(Vector2.down, direction);

        debugCenter = center;
        debugSize = size;
        debugAngle = angle;

        results.Clear();

        Physics2D.OverlapBox(
            center,
            size,
            angle,
            new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = bigRockLayer
            },
            results
        );

        foreach (Collider2D collider in results)
        {
            if (collider == null) continue;

            BigRock bigRock = collider.GetComponentInParent<BigRock>();
            if (bigRock == null) continue;

            GameObject rockObject = bigRock.gameObject;

            if (disabledRocks.Contains(rockObject)) continue;

            disabledRocks.Add(rockObject);
            rockObject.SetActive(false);
        }
    }

    public void RestoreRocks()
    {
        for (int i = disabledRocks.Count - 1; i >= 0; i--)
        {
            if (disabledRocks[i] != null)
                disabledRocks[i].SetActive(true);
        }

        disabledRocks.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix =
            Matrix4x4.TRS(
                debugCenter,
                Quaternion.Euler(0f, 0f, debugAngle),
                Vector3.one
            );

        Gizmos.DrawWireCube(Vector3.zero, debugSize);

        Gizmos.matrix = oldMatrix;
    }
}