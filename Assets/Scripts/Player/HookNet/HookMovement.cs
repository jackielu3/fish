using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class HookMovement : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds3 = new(3f);

    [Header("Values")]
    [SerializeField][ReadOnly] private Vector2 initialTransform;
    [SerializeField][ReadOnly] private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField][ReadOnly] private bool isMoving;

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
        rb.MovePosition(rb.position + movement);
    }

    private void Turn()
    {
        float turn = moveInput.x * turnSpeed * Time.fixedDeltaTime;

        rb.MoveRotation(rb.rotation + turn);
    }

    public void StopHook(GameObject brushInstance)
    {
        isMoving = false;
        StartCoroutine(StopHookRoutine(brushInstance));
    }

    private IEnumerator StopHookRoutine(GameObject brushInstance) {
        moveSpeed = 0;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        Collider2D hookCollider = GetComponent<Collider2D>();

        if (hookCollider != null)
            hookCollider.enabled = false;

        yield return _waitForSeconds3;

        if (brushInstance != null)
            Destroy(brushInstance);

        onControlSwitch.Raise(this, "Boat");

        Destroy(gameObject);
    }

    public Vector2 GetInitialTransform()
    {
        return initialTransform;
    }

    private bool GetIsMoving()
    {
        return isMoving;
    }
}
