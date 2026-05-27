using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NetVisualAnimator : MonoBehaviour
{
    [Header("References")]
    private LineRenderer netLineRenderer;
    private LineRenderer ropeLineRenderer;
    private Transform boatRopePoint;

    [SerializeField] private NetHatchVisual hatchVisual;

    [Header("Shape")]
    [SerializeField] private int visualPointCount = 64;
    [SerializeField] private float minWidth = 1f;
    [SerializeField] private float minLength = 1f;
    [SerializeField] private float sizePadding = 0.25f;

    private int ropePointCount;

    [Header("Animation")]
    [SerializeField] private float morphDuration = 1f;

    private void Awake()
    {
        netLineRenderer = GetComponent<LineRenderer>();

        if (netLineRenderer == null) return;

        if (hatchVisual == null)
            hatchVisual = GetComponentInChildren<NetHatchVisual>();


        GameObject childObj = new("LineRendererChild");
        childObj.transform.SetParent(this.transform);
        childObj.transform.localPosition = Vector3.zero;

        ropeLineRenderer = childObj.AddComponent<LineRenderer>();

        ropeLineRenderer.sharedMaterial = netLineRenderer.sharedMaterial;
        ropeLineRenderer.widthCurve = netLineRenderer.widthCurve;
        ropeLineRenderer.startWidth = netLineRenderer.startWidth;
        ropeLineRenderer.endWidth = netLineRenderer.endWidth;
        ropeLineRenderer.colorGradient = netLineRenderer.colorGradient;
        ropeLineRenderer.sortingLayerID = netLineRenderer.sortingLayerID;
        ropeLineRenderer.useWorldSpace = netLineRenderer.useWorldSpace;
    }

    public void PlayNetMorph(List<Vector2> rawLoopPoints, List<Vector2> rawRopePoints, Transform ropePoint, NetArea netArea)
    {
        boatRopePoint = ropePoint;

        StopAllCoroutines();
        StartCoroutine(MorphRoutine(rawLoopPoints, rawRopePoints, netArea));
    }

    private IEnumerator MorphRoutine(List<Vector2> rawLoopPoints, List<Vector2> rawRopePoints, NetArea netArea)
    {
        List<Vector3> startPoints = ResampleClosedLoop(rawLoopPoints, visualPointCount);
        List<Vector3> targetPoints = BuildTeardropPoints(startPoints);

        Vector3 ropeEnd = rawRopePoints[^1];
        RotatePoints(startPoints, ropeEnd);

        if (Mathf.Sign(GetSignedArea(startPoints)) != Mathf.Sign(GetSignedArea(targetPoints)))
        {
            ReversePoints(targetPoints);
        }

        Vector3 teardropTip = targetPoints[0];

        OffsetPoints(targetPoints, ropeEnd - teardropTip);
        teardropTip = ropeEnd;

        netLineRenderer.positionCount = visualPointCount + 1;

        ropePointCount = rawRopePoints.Count;
        List<Vector3> startRopePoints = ResampleOpenLine(rawRopePoints, ropePointCount);
        List<Vector3> targetRopePoints = BuildStraightRopePoints(boatRopePoint.position, teardropTip, ropePointCount);

        int actualRopePointCount = Mathf.Min(startRopePoints.Count, targetRopePoints.Count);
        ropeLineRenderer.positionCount = actualRopePointCount;

        float timer = 0f;

        while (timer < morphDuration)
        {
            List<Vector3> currentNetPoints = new();

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / morphDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < visualPointCount; i++)
            {
                Vector3 point = Vector3.Lerp(startPoints[i], targetPoints[i], easedT);
                currentNetPoints.Add(point);
                netLineRenderer.SetPosition(i, point);
            }

            netLineRenderer.SetPosition(visualPointCount, netLineRenderer.GetPosition(0));

            Vector3 center = GetCenter(currentNetPoints);
            Vector3 directionToBoat = (boatRopePoint.position - center).normalized;

            if (hatchVisual != null)
                hatchVisual.UpdateHatch(currentNetPoints, directionToBoat, easedT);

            if (netArea != null)
                netArea.UpdateShape(currentNetPoints);

            targetRopePoints = BuildStraightRopePoints(boatRopePoint.position, teardropTip, ropePointCount);

            for (int i = 0; i < actualRopePointCount; i++)
            {
                Vector3 ropePoint = Vector3.Lerp(startRopePoints[i], targetRopePoints[i], easedT);
                ropeLineRenderer.SetPosition(i, ropePoint);
            }

            yield return null;
        }

        for (int i = 0; i < visualPointCount; i++)
        {
            netLineRenderer.SetPosition(i, targetPoints[i]);
        }

        netLineRenderer.SetPosition(visualPointCount, targetPoints[0]);

        targetRopePoints = BuildStraightRopePoints(boatRopePoint.position, teardropTip, ropePointCount);

        for (int i = 0; i < actualRopePointCount; i++)
        {
            ropeLineRenderer.SetPosition(i, targetRopePoints[i]);
        }
    }

    private List<Vector3> BuildTeardropPoints(List<Vector3> sourcePoints)
    {
        Bounds bounds = new(sourcePoints[0], Vector3.zero);

        foreach (Vector3 point in sourcePoints)
        {
            bounds.Encapsulate(point);
        }

        Vector3 center = bounds.center;

        Vector3 boatPos = boatRopePoint.position;
        Vector3 directionToBoat = (boatPos - center).normalized;

        if (directionToBoat == Vector3.zero)
            directionToBoat = Vector3.up;

        Vector3 sideDirection = new(-directionToBoat.y, directionToBoat.x, 0f);
        float width = GetSizeAlongAxis(sourcePoints, sideDirection) + sizePadding;
        float length = GetSizeAlongAxis(sourcePoints, directionToBoat) + sizePadding;

        width = Mathf.Max(width, minWidth);
        length = Mathf.Max(length, minLength);

        Vector3 tip = center + directionToBoat * (length * 0.5f);
        Vector3 bottom = center - directionToBoat * (length * 0.5f);

        List<Vector3> points = new();

        for (int i = 0; i < visualPointCount; i++)
        {
            float normalized = i / (float)visualPointCount;
            float angle = normalized * Mathf.PI * 2;

            float y = Mathf.Cos(angle);
            float x = Mathf.Sin(angle);

            float widthAtPoint = Mathf.Lerp(0.15f, 1f, (1f - y) * 0.5f);
            widthAtPoint *= width;

            Vector3 along = Vector3.Lerp(tip, bottom, (1f - y) * 0.5f);
            Vector3 side = 0.5f * widthAtPoint * x * sideDirection;

            points.Add(along + side);
        }

        return points;
    }

    private List<Vector3> ResampleClosedLoop(List<Vector2> points, int count)
    {
        List<Vector3> result = new();

        if (points.Count < 2) return result;

        float totalLength = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Count];
            totalLength += Vector2.Distance(a, b);
        }

        float spacing = totalLength / count;

        int segmentIndex = 0;
        // float distanceIntoSegment = 0f;

        Vector2 current = points[0];
        result.Add(current);

        for (int i = 1; i < count; i++)
        {
            float targetDistance = spacing;

            while (targetDistance > 0f)
            {
                Vector2 a = points[segmentIndex];
                Vector2 b = points[(segmentIndex + 1) % points.Count];

                float segmentLength = Vector2.Distance(current, b);
                float remaining = targetDistance - segmentLength;

                if (remaining <= 0f)
                {
                    current = Vector2.Lerp(current, b, targetDistance / segmentLength);
                    result.Add(current);
                    targetDistance = 0f;
                }
                else
                {
                    targetDistance = remaining;
                    segmentIndex = (segmentIndex + 1) % points.Count;
                    current = points[segmentIndex];
                }
            }
        }

        return result;
    }

    private List<Vector3> ResampleOpenLine(List<Vector2> points, int count)
    {
        List<Vector3> result = new();

        if (points == null || points.Count == 0 || count <= 0)
            return result;

        if (points.Count == 1 || count == 1)
        {
            result.Add(points[0]);
            return result;
        }

        float totalLength = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            totalLength += Vector2.Distance(points[i], points[i + 1]);
        }

        if (totalLength <= 0.0001f)
        {
            for (int i = 0; i < count; i++)
            {
                result.Add(points[0]);
            }

            return result;
        }

        result.Add(points[0]);

        float spacing = totalLength / (count - 1);
        float distanceTraveled = 0f;
        int segmentIndex = 0;

        for (int i = 1; i < count - 1; i++)
        {
            float targetDistance = spacing * i;

            while (segmentIndex < points.Count - 2)
            {
                float segmentLength = Vector2.Distance(points[segmentIndex], points[segmentIndex + 1]);

                if (distanceTraveled + segmentLength >= targetDistance)
                    break;

                distanceTraveled += segmentLength;
                segmentIndex++;
            }

            Vector2 a = points[segmentIndex];
            Vector2 b = points[segmentIndex + 1];

            float currentSegmentLength = Vector2.Distance(a, b);
            float distanceIntoSegment = targetDistance - distanceTraveled;

            float t = currentSegmentLength <= 0.0001f
                ? 0f
                : distanceIntoSegment / currentSegmentLength;

            result.Add(Vector2.Lerp(a, b, t));
        }

        result.Add(points[^1]);

        return result;
    }

    private List<Vector3> BuildStraightRopePoints(Vector3 start, Vector3 end, int count)
    {
        List<Vector3> points = new();

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            points.Add(Vector3.Lerp(start, end, t));
        }

        return points;
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

    private void RotatePoints(List<Vector3> points, Vector3 target)
    {
        if (points == null || points.Count == 0) return;

        int closestIndex = 0;
        float closestDistance = Vector3.Distance(points[0], target);

        for (int i = 1; i < points.Count; i++)
        {
            float distance = Vector3.Distance(points[i], target);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        if (closestIndex == 0) return;

        List<Vector3> rotated = new();

        for (int i = 0; i < points.Count; i++)
        {
            int index = (closestIndex + i) % points.Count;
            rotated.Add(points[index]);
        }

        points.Clear();
        points.AddRange(rotated);
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

    private void ReversePoints(List<Vector3> points)
    {
        if (points == null || points.Count <= 2) return;

        Vector3 first = points[0];

        points.RemoveAt(0);
        points.Reverse();
        points.Insert(0, first);
    }

    private void OffsetPoints(List<Vector3> points, Vector3 offset)
    {
        if (points == null) return;

        for (int i = 0; i < points.Count; i++)
        {
            points[i] += offset;
        }
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
