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
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Material hatchMaterial;

    private static readonly int HoleRadiusId = Shader.PropertyToID("_HoleRadius");

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.name = "Net Hatch Mesh";
        meshFilter.mesh = mesh;

        hatchMaterial = new Material(hatchMaterialTemplate);
        meshRenderer.material = hatchMaterial;

        Hide();
    }

    public void UpdateHatch(List<Vector3> netPoints, Vector3 directionToBoat, float revealAmount)
    {
        if (netPoints == null || netPoints.Count < 3) return;

        BuildMesh(netPoints);

        float holeRadius = Mathf.Lerp(1.25f, 0f, revealAmount);
        hatchMaterial.SetFloat(HoleRadiusId, holeRadius);

        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    private void BuildMesh(List<Vector3> points)
    {
        mesh.Clear();

        Vector3 center = GetCenter(points);

        Vector3[] vertices = new Vector3[points.Count + 1];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[points.Count * 3];

        vertices[0] = transform.InverseTransformPoint(center);
        uvs[0] = WorldToTileUv(center);

        for (int i = 0; i < points.Count; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(points[i]);
            uvs[i + 1] = WorldToTileUv(points[i]);
        }

        for (int i = 0; i < points.Count; i++)
        {
            int triIndex = i * 3;

            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i == points.Count - 1 ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private Vector2 WorldToTileUv(Vector3 worldPoint)
    {
        return new Vector2(
            worldPoint.x / tileWorldSize,
            worldPoint.y / tileWorldSize
        );
    }

    private Vector3 GetCenter(List<Vector3> points)
    {
        Vector3 total = Vector3.zero;

        foreach (Vector3 point in points)
        {
            total += point;
        }

        return total / points.Count;
    }
}