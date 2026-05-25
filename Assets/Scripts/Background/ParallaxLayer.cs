using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 startCameraPos;
    private Vector3 startLayerPos;

    [Range(0f, 1f)]
    [SerializeField] private float parallaxStrength = 0.5f;

    [SerializeField] private bool useXBounds;
    [SerializeField] private bool useYBounds;
    [SerializeField] private Vector2 xBounds;
    [SerializeField] private Vector2 yBounds;

    private void Start()
    {
        cameraTransform = Camera.main.transform;

        startCameraPos = cameraTransform.position;
        startLayerPos = this.transform.position;
    }

    private void LateUpdate()
    {
        Vector3 cameraOffset = cameraTransform.position - startCameraPos;

        Vector3 pos = startLayerPos + new Vector3(
            cameraOffset.x * parallaxStrength,
            cameraOffset.y * parallaxStrength,
            0f
        );

        if (useXBounds) pos.x = Mathf.Clamp(pos.x, xBounds.x, xBounds.y);
        if (useYBounds) pos.y = Mathf.Clamp(pos.y, yBounds.x, yBounds.y);

        transform.position = pos;
    }

}