using UnityEngine;
using System.Collections;

public class CameraTeleport : MonoBehaviour
{
    [Header("Cámara a mover")]
    public Camera mainCamera;

    [Header("Posición destino")]
    public Vector3 newCameraPosition;

    [Header("Teleport vinculado (el opuesto)")]
    public CameraTeleport linkedTeleport;

    [Header("Transición suave (opcional)")]
    public bool smoothTransition = true;
    public float speed = 5f;

    private bool canTeleport = true;
    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || !canTeleport || isTransitioning)
            return;

        Debug.Log($"[CameraTeleport] Activando '{gameObject.name}' → cámara a {newCameraPosition}");

        // 🔹 Mover cámara
        if (smoothTransition)
            StartCoroutine(MoveCameraSmoothly());
        else
            mainCamera.transform.position = newCameraPosition;

        // 🔒 Desactivar este teleport
        canTeleport = false;

        // 🔓 Activar el teleport opuesto
        if (linkedTeleport != null)
        {
            linkedTeleport.canTeleport = true;
            Debug.Log($"[CameraTeleport] '{linkedTeleport.name}' activado, '{gameObject.name}' desactivado");
        }
    }

    private IEnumerator MoveCameraSmoothly()
    {
        isTransitioning = true;
        Vector3 startPos = mainCamera.transform.position;
        float t = 0f;

        while (Vector3.Distance(mainCamera.transform.position, newCameraPosition) > 0.05f)
        {
            mainCamera.transform.position = Vector3.Lerp(startPos, newCameraPosition, t);
            t += Time.deltaTime * speed;
            yield return null;
        }

        mainCamera.transform.position = newCameraPosition;
        isTransitioning = false;
        Debug.Log($"[CameraTeleport] Cámara llegó a {newCameraPosition}");
    }
}
