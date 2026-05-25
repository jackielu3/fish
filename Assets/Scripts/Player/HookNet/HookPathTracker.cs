using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HookMovement))]
public class HookPathTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HookMovement hookMovement;
    private NetVisualAnimator netVisualAnimator;
    private Transform boatRopePoint;

    [Header("Line Renderer")]
    private LineRenderer currentLineRenderer;
    [SerializeField] private GameObject brushPrefab;
    private GameObject brushInstance;
    [SerializeField] private float pointSpacing = 0.1f;
    [SerializeField] private int recentSegmentsToIgnore = 3;
    private Vector2 lastPoint;

    [Header("Collision")]
    private EdgeCollider2D edgeCollider;
    [SerializeField][ReadOnly] private List<Vector2> cachedPoints = new();

    [Header("Net Area")]
    [SerializeField] private GameObject netAreaPrefab;

    [Header("Events")]
    [SerializeField] private GameEvent onNetCreated;

    private bool hasFinished = false;

    private void Awake()
    {
        hookMovement = GetComponent<HookMovement>();
        CreateBrush();
    }


    private void Update()
    {
        if (hasFinished)
            return;

        Draw();
    }

    public void Initialize(Transform boatPoint)
    {
        boatRopePoint = boatPoint;
    }

    private void CreateBrush()
    {
        brushInstance = Instantiate(brushPrefab);
        brushInstance.transform.position = Vector3.zero;
        brushInstance.transform.rotation = Quaternion.identity;

        brushInstance.SetActive(true);
        netVisualAnimator = brushInstance.GetComponent<NetVisualAnimator>();

        if (netVisualAnimator == null)
        {
            Debug.LogError("Brush prefab is missing a NetVisualAnimator.", brushPrefab);
            enabled = false;
            return;
        }

        currentLineRenderer = brushInstance.GetComponent<LineRenderer>();
        edgeCollider = brushInstance.GetComponent<EdgeCollider2D>();

        if (currentLineRenderer == null)
        {
            Debug.LogError("Brush prefab is missing a LineRenderer.", brushPrefab);
            enabled = false;
            return;
        }

        if (edgeCollider == null)
        {
            Debug.LogError("Brush prefab is missing an EdgeCollider2D.", brushPrefab);
            enabled = false;
            return;
        }

        currentLineRenderer.useWorldSpace = true;

        Vector2 startPos = transform.position;

        cachedPoints.Clear();
        cachedPoints.Add(startPos);
        cachedPoints.Add(startPos);

        edgeCollider.points = cachedPoints.ToArray();

        lastPoint = startPos;

        RefreshLineRenderer();
        RefreshEdgeCollider();
    }

    public void Draw()
    {
        Vector2 currentPos = transform.position;

        if (Vector2.Distance(lastPoint, currentPos) < pointSpacing)
            return;

        AddPoint(currentPos);
    }

    private void AddPoint(Vector2 pointPos)
    {
        Vector2 previousPoint = cachedPoints[^1];

        if (CheckForIntersection(previousPoint, pointPos, out Vector2 intersection, out int intersectedIndex)) // MAYBE ADD A CHECK FOR NUMBER OF NETS
        {
            FinishPath(intersection, intersectedIndex);
            return;
        }

        cachedPoints.Add(pointPos);
        lastPoint = pointPos;

        RefreshLineRenderer();
        RefreshEdgeCollider();
    }

    private bool CheckForIntersection(Vector2 newA, Vector2 newB, out Vector2 intersection, out int intersectedIndex)
    {
        intersection = Vector2.zero;
        intersectedIndex = -1;

        if (cachedPoints.Count < 5)
            return false;

        int newestSegmentIndex = cachedPoints.Count - 1;
        int lastIndexToCheck = Mathf.Max(0, newestSegmentIndex - recentSegmentsToIgnore);

        for (int i = 0; i < lastIndexToCheck; i++)
        {
            Vector2 oldA = cachedPoints[i];
            Vector2 oldB = cachedPoints[i + 1];

            if (LineSegmentsIntersect(newA, newB, oldA, oldB, out intersection))
            {
                Debug.Log("Intersection detected!");
                intersectedIndex = i;
                return true;
            }
        }
        return false;
    }

    private void FinishPath(Vector2 intersection, int intersectedIndex)
    {
        hasFinished = true;

        cachedPoints.Add(intersection);
        lastPoint = intersection;

        RefreshLineRenderer();
        RefreshEdgeCollider();

        List<Vector2> loopPoints = BuildLoopPoints(intersection, intersectedIndex);

        NetArea netAreaInstance = Instantiate(netAreaPrefab).GetComponent<NetArea>();
        netAreaInstance.Initialize(loopPoints);

        List<Vector2> ropePoints = BuildRopePoints(intersection, intersectedIndex);

        netVisualAnimator.PlayNetMorph(loopPoints, ropePoints, boatRopePoint);

        onNetCreated.Raise(this, new List<Vector2>(loopPoints));

        hookMovement.StopHook(brushInstance);
    }

    private List<Vector2> BuildLoopPoints(Vector2 intersection, int intersectedIndex)
    {
        List<Vector2> loopPoints = new();

        loopPoints.Add(intersection);

        for (int i = intersectedIndex + 1; i < cachedPoints.Count - 1; i++)
        {
            loopPoints.Add(cachedPoints[i]);
        }

        return loopPoints;
    }

    private void RefreshLineRenderer()
    {
        currentLineRenderer.positionCount = cachedPoints.Count;
        for (int i = 0; i < cachedPoints.Count; i++)
        {
            currentLineRenderer.SetPosition(i, cachedPoints[i]);
        }
    }

    private void RefreshEdgeCollider()
    {
        edgeCollider.SetPoints(cachedPoints);
    }

    private bool LineSegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float d = (a1.x - a2.x) * (b1.y - b2.y) - (a1.y - a2.y) * (b1.x - b2.x);

        if (Mathf.Abs(d) < 0.0001f)
            return false;

        float pre = (a1.x * a2.y - a1.y * a2.x);
        float post = (b1.x * b2.y - b1.y * b2.x);

        float x = (pre * (b1.x - b2.x) - (a1.x - a2.x) * post) / d;
        float y = (pre * (b1.y - b2.y) - (a1.y - a2.y) * post) / d;

        if (
            x < Mathf.Min(a1.x, a2.x) || x > Mathf.Max(a1.x, a2.x) ||
            x < Mathf.Min(b1.x, b2.x) || x > Mathf.Max(b1.x, b2.x) ||
            y < Mathf.Min(a1.y, a2.y) || y > Mathf.Max(a1.y, a2.y) ||
            y < Mathf.Min(b1.y, b2.y) || y > Mathf.Max(b1.y, b2.y)
            )
            return false;

        intersection = new Vector2(x, y);
        return true;
    }

    private List<Vector2> BuildRopePoints(Vector2 intersection, int intersectedIndex)
    {
        List<Vector2> ropePoints = new();

        for (int i = 0; i <= intersectedIndex; i++)
        {
            ropePoints.Add(cachedPoints[i]);
        }

        ropePoints.Add(intersection);

        return ropePoints;
    }

    public void AddEdgePoint(Vector2 point)
    {
        cachedPoints.Add(point);
        edgeCollider.SetPoints(cachedPoints);
    }
}
