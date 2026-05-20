using UnityEngine;

public class WaterBob : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private float bobAmount = 0.1f;
    [SerializeField] private float bobSpeed = 1f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;        
    }

    // Update is called once per frame
    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        transform.localPosition = startPosition + new Vector3(0f, yOffset, 0f);
    }
}
