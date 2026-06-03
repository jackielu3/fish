using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private Transform cameraTarget;

    [Header("Net Focus")]
    [SerializeField] private float padding = 2f;
    [SerializeField] private float focusDuration = 2f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;

    private Vector3 originalTargetPosition;
    private float originalOrthoSize;

    private Transform followTarget;

    private Coroutine focusRoutine;

    private void Awake()
    {
        vcam.Follow = cameraTarget;

        originalTargetPosition = cameraTarget.position;
        originalOrthoSize = vcam.Lens.OrthographicSize;
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;
        if (focusRoutine != null) return;

        Vector3 targetPosition = followTarget.position;
        targetPosition.z = cameraTarget.position.z;

        cameraTarget.position = targetPosition;

        vcam.Lens.OrthographicSize = Mathf.Lerp(
            vcam.Lens.OrthographicSize,
            originalOrthoSize,
            Time.deltaTime * zoomSpeed
        );
    }

    public void FocusOnNet(Component sender, object data)
    {
        if (data is not List<Vector2> points) return;
        if (focusRoutine != null) StopCoroutine(focusRoutine);

        focusRoutine = StartCoroutine(FocusOnNetRoutine(points));
    }

    private IEnumerator FocusOnNetRoutine(List<Vector2> points)
    {
        followTarget = null;

        Bounds bounds = GetBounds(points);

        Vector3 targetPosition = bounds.center;
        targetPosition.z = cameraTarget.position.z;

        float targetOrthoSize = Mathf.Max(bounds.extents.y, bounds.extents.x / vcam.Lens.Aspect) + padding;

        float timer = 0f;

        while (timer < focusDuration)
        {
            timer += Time.deltaTime;

            cameraTarget.position = Vector3.Lerp(
                cameraTarget.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );

            vcam.Lens.OrthographicSize = Mathf.Lerp(
                vcam.Lens.OrthographicSize,
                targetOrthoSize,
                Time.deltaTime * zoomSpeed
            );

            yield return null;
        }

        focusRoutine = null;
    }

    private Bounds GetBounds(List<Vector2> points)
    {
        Bounds bounds = new(points[0], Vector3.zero);

        foreach (Vector2 point in points)
        {
            bounds.Encapsulate(point);
        }

        return bounds;
    }

    public void FollowTarget(Transform target)
    {
        // Debug.Log("SWITCHING CAMERA TARGETS: " + target.name);

        followTarget = target;
    }
}
