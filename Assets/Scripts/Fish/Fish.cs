using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Fish : MonoBehaviour
{
    [Header("Events")]
    public GameEvent onMoneyEarned;

    private Rigidbody2D rb;

    [Header("Values")]
    [SerializeField] private int totalValue = 10;
    [SerializeField] private float speed = 5f;
    private Vector2 movementDirection = Vector2.left;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + speed * Time.fixedDeltaTime * movementDirection;
        rb.MovePosition(targetPosition);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Fish hit SOMETHING: " + collision.gameObject.name);

        if (collision.collider.CompareTag("Wall"))
        {
            Debug.Log("Fish hit wall!");
            movementDirection = -movementDirection;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    public void Catch()
    {
        Debug.Log("Fish " + name + " caught in net!");
        onMoneyEarned.Raise(this, totalValue);
        Destroy(gameObject);
    }
}
