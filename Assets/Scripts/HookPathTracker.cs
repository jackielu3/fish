using NUnit.Framework.Constraints;
using UnityEngine;

public class HookPathTracker : MonoBehaviour
{
    [SerializeField] private HookMovement hookMovement;

    [SerializeField] private GameObject brushPrefab;
    [SerializeField] private float pointSpacing = 0.1f;

    [SerializeField] private int recentSegmentsToIgnore = 3;

    private LineRenderer currentLineRenderer;
    private Vector3 lastPoint;

    private void Awake()
    {
        hookMovement = GetComponent<HookMovement>();
        CreateLineRenderer();
    }


    private void Update()
    {
        Draw();
    }

    private void CreateLineRenderer()
    {
        GameObject brushInstance = Instantiate(brushPrefab);
        currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        currentLineRenderer.useWorldSpace = true;

        Vector3 startPos = transform.position;
        currentLineRenderer.SetPosition(0, startPos);
        currentLineRenderer.SetPosition(1, startPos);

        lastPoint = startPos;
    }

    public void Draw()
    {
        Vector3 currentPos = transform.position;

        if (Vector3.Distance(lastPoint, currentPos) < pointSpacing)
            return;

        AddPoint(currentPos);
        lastPoint = currentPos;

        CheckForIntersection();
    }

    private void AddPoint(Vector3 pointPos)
    {
        currentLineRenderer.positionCount++;
        int positionIndex = currentLineRenderer.positionCount - 1;
        currentLineRenderer.SetPosition(positionIndex, pointPos);
    }

    private void CheckForIntersection()
    {
        if (currentLineRenderer.positionCount < 5)
            return;

        int newest = currentLineRenderer.positionCount - 1;

        Vector2 newA = currentLineRenderer.GetPosition(newest - 1);
        Vector2 newB = currentLineRenderer.GetPosition(newest);

        int lastIndexToCheck = newest - 1 - recentSegmentsToIgnore;

        for (int i = 0; i < lastIndexToCheck; i++)
        {
            Vector2 oldA = currentLineRenderer.GetPosition(i);
            Vector2 oldB = currentLineRenderer.GetPosition(i + 1);

            if (LineSegmentsIntersect(newA, newB, oldA, oldB, out Vector2 intersection))
            {
                Debug.Log("Intersection detected!");

                hookMovement.StopHook();
                return;
            }
        }
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
}
