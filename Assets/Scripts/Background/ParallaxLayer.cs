using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    private Transform cameraTransform;

    [Range(0f, 1f)]
    [SerializeField] private float parallaxStrength = 0.5f;

    private Vector3 previousCameraPosition;

    private void Start()
    {
        cameraTransform = Camera.main.transform;

        previousCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 cameraDelta = cameraTransform.position - previousCameraPosition;

        transform.position += new Vector3(
            cameraDelta.x * parallaxStrength,
            cameraDelta.y * parallaxStrength,
            0f
        );

        previousCameraPosition = cameraTransform.position;
    }
}