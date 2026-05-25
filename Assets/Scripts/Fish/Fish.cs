using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Fish : MonoBehaviour
{
    public FishData Data { get; private set; }

    [Header("Events")]
    public GameEvent onMoneyEarned;

    [Header("Movement")]
    [SerializeField] private float movementAngleRandomness = 30;
    [SerializeField] private float minSecsMovement = 0.5f;
    [SerializeField] private float maxSecsMovement = 1.5f;
    [SerializeField] private float minSecsBetweenMovement = 0.5f;
    [SerializeField] private float maxSecsBetweenMovement = 1.5f;

    private FishSpawner owningSpawner;
    private Rigidbody2D rb;
    private bool isMoving = false;
    private float stateTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(FishData data, FishSpawner spawner)
    {
        Data = data;
        owningSpawner = spawner;
    }

    private void FixedUpdate()
    {
        if (Data == null) return;

        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer < 0f)
        {
            isMoving = !isMoving;

            if (isMoving)
            {
                stateTimer = UnityEngine.Random.Range(minSecsMovement, maxSecsMovement);
                ApplyRandomVelocity();
            }
            else
            {
                stateTimer = UnityEngine.Random.Range(minSecsBetweenMovement, maxSecsBetweenMovement);
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void ApplyRandomVelocity()
    {
        float randomAngle = UnityEngine.Random.Range(-movementAngleRandomness, movementAngleRandomness);

        if (UnityEngine.Random.value > 0.5f)
        {
            randomAngle += 180f;
        }

        float rad = randomAngle * Mathf.Deg2Rad;

        Vector2 direction = new(Mathf.Cos(rad), Mathf.Sin(rad));
        rb.linearVelocity = direction * Data.moveSpeed;

        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            FaceMovementDirection();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FishSpawnBoundary boundary = collision.GetComponent<FishSpawnBoundary>();

        if (boundary == null) return;
        if (boundary.Owner != owningSpawner) return;

        BounceOffNormal(boundary.BounceNormal);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Wall")) return;

        Vector2 normal = collision.GetContact(0).normal;
        BounceOffNormal(normal);
    }

    private void BounceOffNormal(Vector2 normal)
    {
        Vector2 currentDirection = rb.linearVelocity.normalized;

        if (currentDirection.sqrMagnitude < 0.01f)
        {
            currentDirection = -transform.right;
        }

        Vector2 reflectedDirection = Vector2.Reflect(currentDirection, normal).normalized;

        rb.linearVelocity = reflectedDirection * Data.moveSpeed;

        FaceMovementDirection();
    }

    private void FaceMovementDirection()
    {
        Vector2 direction = rb.linearVelocity.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Vector3 scale = transform.localScale;

        if (direction.x < 0f)
        {
            scale.y = -Mathf.Abs(scale.y);
        }
        else
        {
            scale.y = Mathf.Abs(scale.y);
        }

        transform.localScale = scale;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void Catch()
    {
        onMoneyEarned.Raise(this, Data.value);
        Destroy(gameObject);
    }

    public bool IsMoving() => isMoving;
}