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
    private ContactFilter2D obstacleFilter;

    private UpgradeManager upgradeManager;

    [Header("Events")]
    public GameEvent onControlSwitch;

    private Rigidbody2D rb;
    private readonly RaycastHit2D[] hitResults = new RaycastHit2D[4];

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

    public void Initialize(UpgradeManager manager)
    {
        upgradeManager = manager;

        if (upgradeManager != null)
        {
            moveSpeed = upgradeManager.GetUpgradeValue(UpgradeType.HookSpeed);
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

    private void Move()
    {
        Vector2 movement = moveSpeed * Time.fixedDeltaTime * -transform.up;

        if (movement.sqrMagnitude <= 0.000001f)
            return;

        Vector2 direction = movement.normalized;
        float distance = movement.magnitude;

        int hitCount = rb.Cast(
            direction,
            obstacleFilter,
            hitResults,
            distance + collisionSkin
        );

        if (hitCount > 0)
        {
            RaycastHit2D closestHit = hitResults[0];

            for (int i = 1; i < hitCount; i++)
            {
                if (hitResults[i].distance < closestHit.distance)
                    closestHit = hitResults[i];
            }

            if (((1 << closestHit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                Vector2 slideMovement = Vector2.Perpendicular(closestHit.normal);

                if (Vector2.Dot(slideMovement, movement) < 0f)
                    slideMovement = -slideMovement;

                Vector2 targetPosition = rb.position + slideMovement * distance;

                if (useBounds)
                {
                    targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
                }

                rb.MovePosition(targetPosition);
                return;
            }
        }

        Vector2 normalTargetPosition = rb.position + movement;

        if (useBounds)
        {
            normalTargetPosition.x = Mathf.Clamp(normalTargetPosition.x, minBounds.x, maxBounds.x);
            normalTargetPosition.y = Mathf.Clamp(normalTargetPosition.y, minBounds.y, maxBounds.y);
        }

        rb.MovePosition(normalTargetPosition);
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

    public Vector2 GetInitialTransform()
    {
        return initialTransform;
    }
}
