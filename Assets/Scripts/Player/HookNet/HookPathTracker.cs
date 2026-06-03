using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LineLengthTier
{
    public string tierName;
    public float maxLineLength;
}

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

    [SerializeField] private float intersectionEpsilon = 0.03f;

    [Header("Collision")]
    private EdgeCollider2D edgeCollider;
    [SerializeField][ReadOnly] private List<Vector2> cachedPoints = new();

    [Header("Net Area")]
    [SerializeField] private GameObject netAreaPrefab;

    private UpgradeManager upgradeManager;

    [Header("Line Length")]
    [SerializeField][ReadOnly] private float lineUsed;
    [SerializeField][ReadOnly] private float lineRemaining;

    public float LineUsed => lineUsed;
    public float MaxLineLength =>
        upgradeManager == null ? 20f : upgradeManager.GetUpgradeValue(UpgradeType.RopeLength);
    public float LineRemaining => lineRemaining;

    [Header("Events")]
    [SerializeField] private GameEvent onNetCreated;

    private bool hasFinished = false;

    private void Awake()
    {
        hookMovement = GetComponent<HookMovement>();
        CreateBrush();
    }

    private void LateUpdate()
    {
        if (hasFinished)
            return;

        Draw();
    }

    public void Initialize(Transform boatPoint, UpgradeManager manager)
    {
        boatRopePoint = boatPoint;
        upgradeManager = manager;

        lineUsed = 0f;
        lineRemaining = MaxLineLength;
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

        float addedLength = Vector2.Distance(previousPoint, pointPos);

        if (lineUsed + addedLength >= MaxLineLength)
        {
            OutOfLine();
            return;
        }

        lineUsed += addedLength;
        lineRemaining = MaxLineLength - lineUsed;

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

    private void OutOfLine()
    {
        hasFinished = true;

        hookMovement.StopHook();

        if (brushInstance != null)
            Destroy(brushInstance);

        hookMovement.onControlSwitch.Raise(this, "Boat");

        Destroy(gameObject);

    }

    private bool CheckForIntersection(Vector2 newA, Vector2 newB, out Vector2 intersection, out int intersectedIndex)
    {
        intersection = Vector2.zero;
        intersectedIndex = -1;

        int newestSegmentIndex = cachedPoints.Count - 1;
        int lastIndexToCheck = Mathf.Max(0, newestSegmentIndex - recentSegmentsToIgnore);

        float closestT = float.MaxValue;

        for (int i = 0; i < lastIndexToCheck; i++)
        {
            Vector2 oldA = cachedPoints[i];
            Vector2 oldB = cachedPoints[i + 1];

            if (Vector2.Distance(oldA, oldB) <= 0.0001f)
                continue;

            if (LineSegmentsIntersect(newA, newB, oldA, oldB, out Vector2 hit, out float tOnNewSegment))
            {
                if (tOnNewSegment < closestT)
                {
                    closestT = tOnNewSegment;
                    intersection = hit;
                    intersectedIndex = i;
                }
            }
        }
        return intersectedIndex != -1;
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
        netAreaInstance.transform.SetParent(this.transform);
        netAreaInstance.Initialize(loopPoints);

        List<Vector2> ropePoints = BuildRopePoints(intersection, intersectedIndex);

        onNetCreated.Raise(this, new List<Vector2>(loopPoints));

        hookMovement.StopHook();

        netVisualAnimator.PlayNetMorph(
            loopPoints,
            ropePoints,
            boatRopePoint,
            netAreaInstance,
            () => ShowCatchResults(netAreaInstance)
        );
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

    private bool LineSegmentsIntersect(
        Vector2 a1,
        Vector2 a2,
        Vector2 b1,
        Vector2 b2,
        out Vector2 intersection,
        out float tOnA
        )
    {
        intersection = Vector2.zero;
        tOnA = 0f;

        Vector2 r = a2 - a1;
        Vector2 s = b2 - b1;

        float rxs = Cross(r, s);
        float qpxr = Cross(b1 - a1, r);

        if (r.sqrMagnitude <= 0.000001f || s.sqrMagnitude <= 0.000001f)
            return false;

        if (Mathf.Abs(rxs) <= 0.000001f)
        {
            if (Mathf.Abs(qpxr) > intersectionEpsilon)
                return false;

            float t0 = Vector2.Dot(b1 - a1, r) / Vector2.Dot(r, r);
            float t1 = Vector2.Dot(b2 - a1, r) / Vector2.Dot(r, r);

            float minT = Mathf.Min(t0, t1);
            float maxT = Mathf.Max(t0, t1);

            if (maxT < -intersectionEpsilon || minT > 1f + intersectionEpsilon)
                return false;

            tOnA = Mathf.Clamp01(minT);
            intersection = a1 + r * tOnA;
            return true;
        }

        float t = Cross(b1 - a1, s) / rxs;
        float u = Cross(b1 - a1, r) / rxs;

        if (
            t < -intersectionEpsilon || t > 1f + intersectionEpsilon ||
            u < -intersectionEpsilon || u > 1f + intersectionEpsilon
        )
        {
            return false;
        }

        tOnA = Mathf.Clamp01(t);
        intersection = a1 + r * tOnA;
        return true;
    }

    private float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
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

    private void ShowCatchResults(NetArea netArea)
    {
        CatchResult result = netArea.CatchFish();

        UIManager.Instance.CatchResultsUI.Show(result, () =>
        {
            netArea.DestroyCaughtFish();

            if (brushInstance != null)
                Destroy(brushInstance);

            hookMovement.onControlSwitch.Raise(this, "Boat");

            Destroy(gameObject);
        });
    }
}
