using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoatMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private Transform hookSpawn;
    [SerializeField] private BaitInventoryManager baitInventoryManager;

    [SerializeField] private BoatManager boatManager;
    [SerializeField] private SpriteRenderer boatSpriteRenderer;

    [SerializeField] private UpgradeManager upgradeManager;

    public Vector2 HookSpawnPosition => hookSpawn.position;

    private Rigidbody2D rb;

    [Header("Values")]
    [SerializeField][ReadOnly] private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        RefreshActiveBoatVisual();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void RefreshActiveBoatVisual()
    {
        BoatData activeBoat = boatManager.ActiveBoat;

        if (activeBoat == null)
            return;

        boatSpriteRenderer.gameObject.SetActive(true);
        boatSpriteRenderer.sprite = activeBoat.boatSprite;
        hookSpawn.localPosition = activeBoat.hookSpawnLocalPosition;
    }

    public void SetMoveInput(Vector2 value)
    {
        moveInput = value;
    }

    private void Move()
    {
        Vector2 movement = moveSpeed * Time.fixedDeltaTime * moveInput;
        rb.MovePosition(rb.position + movement);
    }

    public GameObject LaunchHook(Vector2 launchDirection, float initialDashDistance)
    {
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f);
        GameObject hook = Instantiate(hookPrefab, hookSpawn.position, spawnRotation);

        HookMovement hookMovement = hook.GetComponent<HookMovement>();
        HookPathTracker hookPathTracker = hook.GetComponent<HookPathTracker>();
        HookInitialDive hookInitialDive = hook.GetComponent<HookInitialDive>();

        HookBaitDropper baitDropper = hook.GetComponent<HookBaitDropper>();

        if (baitDropper != null)
            baitDropper.Initialize(baitInventoryManager);

        hookMovement.Initialize(upgradeManager, boatManager);
        hookMovement.FaceDirection(launchDirection);
        hookPathTracker.Initialize(hookSpawn, upgradeManager, boatManager);

        hookPathTracker.SetLineUsageEnabled(false);
        hookMovement.SetMovementEnabled(false);

        if (hookInitialDive != null)
        {
            hookInitialDive.BeginInitialDive(
                launchDirection,
                initialDashDistance,
                hookMovement,
                hookPathTracker
            );
        }

        return hook;
    }
}
