using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class BoatMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private Transform hookSpawn;

    [SerializeField] private UpgradeManager upgradeManager;

    private Rigidbody2D rb;

    [Header("Values")]
    [SerializeField][ReadOnly] private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Move();
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

    public GameObject LaunchHook()
    {
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f);
        GameObject hook = Instantiate(hookPrefab, hookSpawn.position, spawnRotation);

        HookMovement hookMovement = hook.GetComponent<HookMovement>();
        HookPathTracker hookPathTracker = hook.GetComponent<HookPathTracker>();

        hookMovement.Initialize(upgradeManager);
        hookPathTracker.Initialize(hookSpawn, upgradeManager);

        return hook;
    }
}
