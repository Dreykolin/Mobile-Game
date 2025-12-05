using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float lifeTime = 0.7f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // CASO 1: Matar al Player
        if (collision.CompareTag("Player"))
        {
            // (Asumiendo que tu player tiene el script de vida que vimos antes)
            collision.GetComponent<PlayerHealth>()?.TakeDamage(999);
        }
        // CASO 2: Matar a los Mobs
        else if (collision.CompareTag("Mobs"))
        {
            Debug.Log("¡Un mob ha sido eliminado!");

            // 👇 AGREGAR ESTO: Dar XP por Kill
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var stats = player.GetComponent<PlayerStats>();
                if (stats != null) stats.AddXP(stats.xpPerKill);
            }

            collision.GetComponent<EnemyHealth>()?.TakeDamage(999);

        }
        // CASO 3 (NUEVO): Derretir Muro de Hielo
        else if (collision.CompareTag("IceWall"))
        {
            Debug.Log("¡Muro de hielo derretido!");
            // Destruimos el objeto del muro de hielo
            Destroy(collision.gameObject);

            // Opcional: Si tu script IceWall tenía algo importante al morir (sonido, partículas),
            // en vez de Destroy(collision.gameObject), podrías llamar a una función pública en él:
            // collision.GetComponent<IceWall>()?.Melt(); 
            // Pero por ahora, el Destroy directo sirve.
        }
        // CASO 4: Reacción en cadena con otras bombas
        else if (collision.CompareTag("Bomb"))
        {
            Bomb bomb = collision.GetComponent<Bomb>();
            if (bomb != null)
            {
                bomb.Explode();
            }
        }
    }
}