using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Fish : MonoBehaviour
{
    public FishData Data { get; private set; }

    [Header("Events")]
    [SerializeField] private GameEvent onMoneyEarned;

    [Header("Movement")]
    [SerializeField] private float movementAngleRandomness = 30;
    [SerializeField] private float minSecsMovement = 0.5f;
    [SerializeField] private float maxSecsMovement = 1.5f;
    [SerializeField] private float minSecsBetweenMovement = 0.5f;
    [SerializeField] private float maxSecsBetweenMovement = 1.5f;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float collisionSkin = 0.02f;

    private readonly RaycastHit2D[] hitResults = new RaycastHit2D[4];
    private ContactFilter2D obstacleFilter;

    public FishSpawner owningSpawner;

    private Rigidbody2D rb;
    private bool isMoving = false;
    private float stateTimer = 0f;

    private NetArea containingNet;
    private float containingNetPadding = 0.2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        obstacleFilter = new ContactFilter2D();
        obstacleFilter.SetLayerMask(obstacleLayer);
        obstacleFilter.useLayerMask = true;
        obstacleFilter.useTriggers = false;
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

        MoveWithCollision();

        KeepInsideSpawnBounds();
        KeepInsideContainingNet();
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

    private void MoveWithCollision()
    {
        Vector2 movement = rb.linearVelocity * Time.fixedDeltaTime;

        if (movement.sqrMagnitude <= 0.000001f)
            return;

        int hitCount = rb.Cast(
            movement.normalized,
            obstacleFilter,
            hitResults,
            movement.magnitude + collisionSkin
        );

        if (hitCount > 0)
        {
            RaycastHit2D closestHit = hitResults[0];

            for (int i = 1; i < hitCount; i++)
            {
                if (hitResults[i].distance < closestHit.distance)
                    closestHit = hitResults[i];
            }

            Vector2 reflected = Vector2.Reflect(movement.normalized, closestHit.normal).normalized;
            rb.linearVelocity = reflected * Data.moveSpeed;

            FaceMovementDirection();
            return;
        }

        rb.MovePosition(rb.position + movement);
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

    private void KeepInsideSpawnBounds()
    {
        if (owningSpawner == null) return;

        Bounds bounds = owningSpawner.GetSpawnBounds();

        Vector2 pos = rb.position;
        Vector2 velocity = rb.linearVelocity;

        bool bounced = false;

        if (pos.x < bounds.min.x)
        {
            pos.x = bounds.min.x;
            velocity.x = Mathf.Abs(velocity.x);
            bounced = true;
        }
        else if (pos.x > bounds.max.x)
        {
            pos.x = bounds.max.x;
            velocity.x = -Mathf.Abs(velocity.x);
            bounced = true;
        }

        if (pos.y < bounds.min.y)
        {
            pos.y = bounds.min.y;
            velocity.y = Mathf.Abs(velocity.y);
            bounced = true;
        }
        else if (pos.y > bounds.max.y)
        {
            pos.y = bounds.max.y;
            velocity.y = -Mathf.Abs(velocity.y);
            bounced = true;
        }

        if (!bounced) return;

        rb.position = pos;

        if (velocity.sqrMagnitude > 0.01f)
        {
            rb.linearVelocity = velocity.normalized * Data.moveSpeed;
            FaceMovementDirection();
        }
    }

    private void KeepInsideContainingNet()
    {
        if (containingNet == null) return;

        if (!containingNet.TryGetInsideCorrection(
            rb.position,
            containingNetPadding,
            out Vector2 correctedPosition,
            out Vector2 inwardNormal
        ))
        {
            return;
        }

        rb.position = correctedPosition;

        Vector2 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude > 0.01f && Vector2.Dot(velocity.normalized, inwardNormal) < 0f)
        {
            rb.linearVelocity = Vector2.Reflect(velocity.normalized, inwardNormal) * Data.moveSpeed;
            FaceMovementDirection();
        }
    }

    public void SetContainingNet(NetArea net, float padding)
    {
        containingNet = net;
        containingNetPadding = padding;
    }

    public void Catch()
    {
        Data.numberCaught++;
    }

    public bool IsMoving() => isMoving;
}