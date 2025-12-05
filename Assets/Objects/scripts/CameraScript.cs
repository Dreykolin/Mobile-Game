using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target a seguir")]
    public Transform target;

    [Header("Ajustes de cámara")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 0f, -10f); 

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;

    }
}
