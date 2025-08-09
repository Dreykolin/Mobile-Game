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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica si la colisión es con el jugador
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡El jugador ha muerto!");
            Destroy(collision.gameObject);
        }
        // NUEVA LÓGICA: Verifica si la colisión es con una bomba
        else if (collision.CompareTag("Bomb"))
        {
            // Esta línea de Debug.Log te dirá si la explosión tocó una bomba
            Debug.Log("¡Explosión tocó una bomba! Detonando...");

            // Intenta obtener el componente Bomb.cs del objeto colisionado
            Bomb bomb = collision.GetComponent<Bomb>();

            // Si tiene el componente y no ha explotado, hazla explotar
            if (bomb != null)
            {
                bomb.Explode();
            }
        }
    }
}