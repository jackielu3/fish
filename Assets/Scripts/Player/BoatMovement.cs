using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoatMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private BaitInventoryManager baitInventoryManager;
    [SerializeField] private LoanManager loanManager;

    [SerializeField] private BoatManager boatManager;
    [SerializeField] private UpgradeManager upgradeManager;

    private PlayerBoatVisual activePlayerBoat;
    private Transform hookSpawn;

    public Vector2 HookSpawnPosition =>
        hookSpawn != null ? hookSpawn.position : transform.position;

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
        foreach (BoatManager.BoatEntry entry in boatManager.GetAllBoatEntries())
        {
            if (entry.playerBoatObject != null)
                entry.playerBoatObject.gameObject.SetActive(false);
        }

        BoatManager.BoatEntry activeEntry = boatManager.ActiveBoatEntry;

        if (activeEntry == null || activeEntry.playerBoatObject == null)
        {
            activePlayerBoat = null;
            hookSpawn = null;
            return;
        }

        activePlayerBoat = activeEntry.playerBoatObject;
        activePlayerBoat.gameObject.SetActive(true);
        activePlayerBoat.SetBoatName(activeEntry.boatData.boatName);

        hookSpawn = activePlayerBoat.HookSpawn;

        if (hookSpawn == null)
            Debug.LogError($"Active boat '{activeEntry.boatData.boatName}' has no Hook Spawn assigned.");
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

    public GameObject LaunchHook(Vector2 launchDirection, float initialDashDistance, float initialDashDuration)
    {
        if (hookSpawn == null)
        {
            Debug.LogError("Cannot launch hook because no active boat hook spawn is assigned.");
            return null;
        }

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
        hookPathTracker.Initialize(hookSpawn, upgradeManager, boatManager, loanManager);

        hookPathTracker.SetLineUsageEnabled(false);
        hookMovement.SetMovementEnabled(false);

        if (hookInitialDive != null)
        {
            hookInitialDive.BeginInitialDive(
                launchDirection,
                initialDashDistance,
                initialDashDuration,
                hookMovement,
                hookPathTracker
            );
        }

        return hook;
    }
}
