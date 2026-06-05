using UnityEngine;

public class LaunchAimIndicator : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Transform arrowPivot;

    [Header("Aim")]
    [SerializeField] private float maxAngle = 45f;
    [SerializeField] private float rotateSpeed = 90f;

    private float currentAngle;

    public Vector2 Direction
    {
        get
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, currentAngle);
            return rotation * Vector2.down;
        }
    }

    private void Awake()
    {
        HideAndReset();
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void HideAndReset()
    {
        currentAngle = 0f;

        if (arrowPivot != null)
            arrowPivot.localRotation = Quaternion.identity;

        if (root != null)
            root.SetActive(false);
    }

    public void UpdateAim(float horizontalInput)
    {
        Show();

        currentAngle += horizontalInput * rotateSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, -maxAngle, maxAngle);

        if (arrowPivot != null)
            arrowPivot.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
}