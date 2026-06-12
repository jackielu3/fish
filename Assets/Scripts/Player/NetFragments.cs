
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NetBitPickup : MonoBehaviour
{
    [SerializeField] private float addedLineLength = 5f;
    [SerializeField] private GameObject collectVisual;

    [ReadOnly] private bool collected;

    public void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;
        
        HookPathTracker tracker = collision.GetComponent<HookPathTracker>();

        if (tracker == null)
            tracker = collision.GetComponentInParent<HookPathTracker>();

        collected = true;

        tracker.AddBonusLine(addedLineLength);

        if (collectVisual != null)
            Instantiate(collectVisual, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}