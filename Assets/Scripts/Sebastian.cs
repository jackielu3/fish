using UnityEngine;

public class Sebastian : MonoBehaviour
{
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float amplitude = 5.0f;

    [SerializeField] private float bobAmount = 0.1f;
    [SerializeField] private float bobSpeed = 1f;

    private Vector3 startPosition;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        startPosition = transform.position;
    }

    void Update()
    {
        float xOffset = Mathf.Sin(Time.time * speed) * amplitude;
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        transform.position = startPosition + new Vector3(xOffset, yOffset, 0);
    }

    void OnBecameVisible()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void OnBecameInvisible()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
}
