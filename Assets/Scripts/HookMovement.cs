using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HookMovement : MonoBehaviour
{
    [Header("Values")]
    [SerializeField][ReadOnly] private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField][ReadOnly] private bool isMoving;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        Debug.Log($"moveInput.x: {moveInput.x}, turn: {turn}, rb rotation: {rb.rotation}");

        rb.MoveRotation(rb.rotation + turn);
    }

    public void StopHook()
    {
        // TEMP
        Destroy(gameObject);
    }

    private bool GetIsMoving()
    {
        return isMoving;
    }
}
   