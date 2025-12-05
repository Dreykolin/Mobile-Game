using UnityEngine;

public class CameraSnap2D : MonoBehaviour
{
    [Header("Target a seguir")]
    public Transform target;

    [Header("Ajustes de cámara")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 0f, -10f); // En 2D el eje Z debe ir hacia atrás

    void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada: misma X/Y que el jugador + desplazamiento
        Vector3 desiredPosition = target.position + offset;

        // Movimiento suave (lerp) hacia el objetivo
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Aplicar posición
        transform.position = smoothedPosition;

        // En 2D no es necesario mirar al target
        // transform.LookAt(target);  ← Eliminado
    }
}
