using UnityEngine;

public class Explosion : MonoBehaviour
{
    // Duraci�n de la explosi�n en segundos
    public float lifeTime = 0.7f;

    void Start()
    {
        // Destruye el objeto de la explosi�n despu�s de su tiempo de vida
        Destroy(gameObject, lifeTime);
    }
}