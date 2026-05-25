using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class NetHatchVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material hatchMaterialTemplate;

    [Header("Tiling")]
    [SerializeField] private float tileWorldSize = 1f;

    private MeshRenderer meshRenderer;
    private Material hatchMaterial;

    private static readonly int WidthId = Shader.PropertyToID("_Width");
    private static readonly int LengthId = Shader.PropertyToID("_Length");
    private static readonly int TileWorldSizeId = Shader.PropertyToID("_TileWorldSize");
    private static readonly int HoleRadiusId = Shader.PropertyToID("_HoleRadius");

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        GetComponent<MeshFilter>().mesh = BuildQuadMesh();

        hatchMaterial = new Material(hatchMaterialTemplate);
        meshRenderer.material = hatchMaterial;

        Hide();
    }

    public void UpdateHatch(List<Vector3> netPoints, Vector3 directionToBoat, float revealAmount)
    {
        if (netPoints == null || netPoints.Count == 0) return;

        Bounds bounds = new(netPoints[0], Vector3.zero);

        foreach (Vector3 point in netPoints)
        {
            bounds.Encapsulate(point);
        }

        Vector3 center = bounds.center;

        if (directionToBoat == Vector3.zero)
            directionToBoat = Vector3.up;

        Vector3 sideDirection = new(-directionToBoat.y, directionToBoat.x, 0f);

        float width = GetSizeAlongAxis(netPoints, sideDirection);
        float length = GetSizeAlongAxis(netPoints, directionToBoat);

        transform.position = center;

        float angle = Mathf.Atan2(directionToBoat.y, directionToBoat.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        transform.localScale = new Vector3(width, length, 1f);

        hatchMaterial.SetFloat(WidthId, width);
        hatchMaterial.SetFloat(LengthId, length);
        hatchMaterial.SetFloat(TileWorldSizeId, tileWorldSize);

        float holeRadius = Mathf.Lerp(1.25f, 0f, revealAmount);
        hatchMaterial.SetFloat(HoleRadiusId, holeRadius);

        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    private Mesh BuildQuadMesh()
    {
        Mesh mesh = new();

        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(-0.5f,  0.5f, 0f),
            new Vector3( 0.5f,  0.5f, 0f),
            new Vector3( 0.5f, -0.5f, 0f)
        };

        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };

        mesh.triangles = new[]
        {
            0, 1, 2,
            0, 2, 3
        };

        mesh.RecalculateBounds();
        return mesh;
    }

    private float GetSizeAlongAxis(List<Vector3> points, Vector3 axis)
    {
        axis.Normalize();

        float min = Vector3.Dot(points[0], axis);
        float max = min;

        for (int i = 1; i < points.Count; i++)
        {
            float value = Vector3.Dot(points[i], axis);

            if (value < min) min = value;
            if (value > max) max = value;
        }

        return max - min;
    }
}