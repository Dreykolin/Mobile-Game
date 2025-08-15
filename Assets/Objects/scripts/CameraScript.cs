using UnityEngine;
public class CameraFollow : MonoBehaviour { 
    public Transform player; 
    public Vector3 offset = new Vector3(0, 0, -10); 
    public float smoothSpeed = 5f; // velocidad de suavizado
    void LateUpdate() { if (player != null) { 
            Vector3 targetPos = player.position + offset; 
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime); 
        } 
    } 
}