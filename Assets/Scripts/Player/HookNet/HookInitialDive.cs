using System.Collections;
using UnityEngine;

public class HookInitialDive : MonoBehaviour
{
    [SerializeField] private float dashDuration = 0.35f;

    private Coroutine dashRoutine;

    public void BeginInitialDive(
        Vector2 direction,
        float distance,
        HookMovement hookMovement,
        HookPathTracker hookPathTracker
    )
    {
        if (dashRoutine != null)
            StopCoroutine(dashRoutine);

        dashRoutine = StartCoroutine(DashRoutine(
            direction.normalized,
            distance,
            hookMovement,
            hookPathTracker
        ));
    }

    private IEnumerator DashRoutine(
        Vector2 direction,
        float distance,
        HookMovement hookMovement,
        HookPathTracker hookPathTracker
    )
    {
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + direction * distance;

        float timer = 0f;

        while (timer < dashDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / dashDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            Vector2 position = Vector2.Lerp(startPosition, targetPosition, easedT);

            hookMovement.MoveWithoutCollision(position);

            yield return new WaitForFixedUpdate();
        }

        hookMovement.MoveWithoutCollision(targetPosition);

        hookPathTracker.SetLineUsageEnabled(true);
        hookMovement.SetMovementEnabled(true);
    }
}