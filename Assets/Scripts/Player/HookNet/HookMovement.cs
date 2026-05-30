using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HookMovement : MonoBehaviour
{
    [Header("Values")]
    [SerializeField][ReadOnly] private Vector2 initialTransform;
    [SerializeField][ReadOnly] private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField][ReadOnly] private bool isMoving;

    [Header("Bounds")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    [Header("Events")]
    public GameEvent onControlSwitch;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialTransform = transform.position;
        isMoving = true;
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
        Vector2 targetPosition = rb.position + movement;

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

    public Vector2 GetInitialTransform()
    {
        return initialTransform;
    }
}
