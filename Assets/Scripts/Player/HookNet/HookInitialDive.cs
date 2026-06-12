using System.Collections;
using UnityEngine;

public class HookInitialDive : MonoBehaviour
{
    private Coroutine dashRoutine;

    public void BeginInitialDive(
        Vector2 direction,
        float distance,
        float duration,
        HookMovement hookMovement,
        HookPathTracker hookPathTracker
    )
    {
        if (dashRoutine != null)
            StopCoroutine(dashRoutine);

        dashRoutine = StartCoroutine(DashRoutine(
            direction.normalized,
            distance,
            Mathf.Max(0.01f, duration),
            hookMovement,
            hookPathTracker
        ));
    }

    private IEnumerator DashRoutine(
        Vector2 direction,
        float distance,
        float duration,
        HookMovement hookMovement,
        HookPathTracker hookPathTracker
    )
    {
        Vector2 startPosition = transform.position;
        float timer = 0f;

        float endSpeed = hookMovement.CurrentMoveSpeed;
        float endSlope = endSpeed * duration / distance;
        endSlope = Mathf.Clamp(endSlope, 0.05f, 2.5f);

        while (timer < duration)
        {
            timer += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float easedT = Hermite01(t, 0f, endSlope);

            Vector2 position = startPosition + direction * (distance * easedT);
            hookMovement.MoveWithoutCollision(position);

            yield return new WaitForFixedUpdate();
        }

        hookMovement.MoveWithoutCollision(startPosition + direction * distance);

        hookPathTracker.SetLineUsageEnabled(true);
        hookMovement.SetMovementEnabled(true);
    }

    private float Hermite01(float t, float startSlope, float endSlope)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float h00 = 2f * t3 - 3f * t2 + 1f;
        float h10 = t3 - 2f * t2 + t;
        float h01 = -2f * t3 + 3f * t2;
        float h11 = t3 - t2;

        return h10 * startSlope + h01 + h11 * endSlope;
    }
}