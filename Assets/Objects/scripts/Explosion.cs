using UnityEngine;

public class Explosion : MonoBehaviour
{
    // Duración de la explosión en segundos
    public float lifeTime = 0.7f;

    void Start()
    {
        // Destruye el objeto de la explosión después de su tiempo de vida
        Destroy(gameObject, lifeTime);
    }
}