using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HookSpeedTier
{
    public string tierName;
    public float moveSpeed;
}

[RequireComponent(typeof(Rigidbody2D))]
public class HookMovement : MonoBehaviour
{
    [Header("Values")]
    [SerializeField][ReadOnly] private Vector2 initialTransform;
    [SerializeField][ReadOnly] private Vector2 moveInput;
    private float moveSpeed;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField][ReadOnly] private bool isMoving;

    [Header("Bounds")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float collisionSkin = 0.02f;
    [SerializeField] private int collisionIterations = 3;
    private ContactFilter2D obstacleFilter;

    private UpgradeManager upgradeManager;

    [Header("Events")]
    public GameEvent onControlSwitch;

    private Rigidbody2D rb;
    private readonly RaycastHit2D[] hitResults = new RaycastHit2D[4];

    public float CurrentMoveSpeed => moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialTransform = transform.position;
        isMoving = true;

        obstacleFilter = new ContactFilter2D();
        obstacleFilter.SetLayerMask(obstacleLayer);
        obstacleFilter.useLayerMask = true;
        obstacleFilter.useTriggers = false;
    }

    public void Initialize(UpgradeManager manager, BoatManager boatManager)
    {
        upgradeManager = manager;

        if (upgradeManager != null)
        {
            moveSpeed = upgradeManager.GetUpgradeValue(UpgradeType.HookSpeed);
        }

        if (boatManager != null)
        {
            moveSpeed += boatManager.GetActiveEffectAmount(BoatEffectType.IncreaseHookTurnSpeed) / 10;
            turnSpeed += boatManager.GetActiveEffectAmount(BoatEffectType.IncreaseHookTurnSpeed) * 10;
        }
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        Move();
        Turn();
    }

    public void SetMoveInput(Vector2 value)
    {
        moveInput = value;
    }

    public void SetMovementEnabled(bool enabled)
    {
        isMoving = enabled;

        if (!enabled)
            moveInput = Vector2.zero;
    }

    public void MoveWithoutCollision(Vector2 targetPosition)
    {
        rb.MovePosition(targetPosition);
    }

    private void Move()
    {
        Vector2 remainingMovement = moveSpeed * Time.fixedDeltaTime * -transform.up;

        for (int i = 0; i < collisionIterations; i++)
        {
            if (remainingMovement.sqrMagnitude <= 0.000001f)
                return;

            Vector2 direction = remainingMovement.normalized;
            float distance = remainingMovement.magnitude;

            int hitCount = rb.Cast(
                direction,
                obstacleFilter,
                hitResults,
                distance + collisionSkin
            );

            if (hitCount == 0)
            {
                MoveTo(rb.position + remainingMovement);
                return;
            }

            RaycastHit2D closestHit = GetClosestHit(hitCount);

            float safeDistance = Mathf.Max(0f, closestHit.distance - collisionSkin);
            Vector2 safeMovement = direction * safeDistance;

            MoveTo(rb.position + safeMovement);

            Vector2 leftoverMovement = remainingMovement - safeMovement;
            remainingMovement =
                leftoverMovement -
                Vector2.Dot(leftoverMovement, closestHit.normal) * closestHit.normal;
        }
    }

    private RaycastHit2D GetClosestHit(int hitCount)
    {
        RaycastHit2D closestHit = hitResults[0];

        for (int i = 1; i < hitCount; i++)
        {
            if (hitResults[i].distance < closestHit.distance)
                closestHit = hitResults[i];
        }

        return closestHit;
    }

    private void MoveTo(Vector2 targetPosition)
    {
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }

        rb.MovePosition(targetPosition);
    }

    private void Turn()
    {
        float turn = moveInput.x * turnSpeed * Time.fixedDeltaTime;

        rb.MoveRotation(rb.rotation + turn);
    }

    public void StopHook()
    {
        isMoving = false;
        moveSpeed = 0f;
        moveInput = Vector2.zero;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        Collider2D hookCollider = GetComponent<Collider2D>();

        if (hookCollider != null)
            hookCollider.enabled = false;
    }

    public void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        rb.MoveRotation(angle);
    }

    public Vector2 GetInitialTransform()
    {
        return initialTransform;
    }
}
