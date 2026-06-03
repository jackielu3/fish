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

    [SerializeField] private bool useDebugSolidMaterial = true;

    private static readonly int HoleRadiusId = Shader.PropertyToID("_HoleRadius");

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.name = "Net Hatch Mesh";
        meshFilter.mesh = mesh;

        if (useDebugSolidMaterial)
        {
            hatchMaterial = new Material(Shader.Find("Sprites/Default"));
            hatchMaterial.color = new Color(1f, 1f, 1f, 0.35f);
        }
        else
        {
            hatchMaterial = new Material(hatchMaterialTemplate);
        }

        meshRenderer.material = hatchMaterial;
        Hide();
    }

    public void UpdateHatch(List<Vector3> netPoints, Vector3 directionToBoat, float revealAmount)
    {
        if (netPoints == null || netPoints.Count < 3) return;

        BuildMesh(netPoints);

        if (!useDebugSolidMaterial)
        {
            float holeRadius = Mathf.Lerp(1.25f, 0f, revealAmount);
            hatchMaterial.SetFloat(HoleRadiusId, holeRadius);
        }

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

        bool isClockwise = GetSignedArea(points) < 0f;

        for (int i = 0; i < points.Count; i++)
        {
            int triIndex = i * 3;

            int current = i + 1;
            int next = i == points.Count - 1 ? 1 : i + 2;

            triangles[triIndex] = 0;

            if (isClockwise)
            {
                triangles[triIndex + 1] = next;
                triangles[triIndex + 2] = current;
            }
            else
            {
                triangles[triIndex + 1] = current;
                triangles[triIndex + 2] = next;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private float GetSignedArea(List<Vector3> points)
    {
        float area = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[(i + 1) % points.Count];

            area += (a.x * b.y) - (b.x * a.y);
        }

        return area * 0.5f;
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