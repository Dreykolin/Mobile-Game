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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica si la colisi�n es con el jugador
        if (collision.CompareTag("Player"))
        {
            Debug.Log("�El jugador ha muerto!");
            Destroy(collision.gameObject);
        }
        // NUEVA L�GICA: Verifica si la colisi�n es con una bomba
        else if (collision.CompareTag("Bomb"))
        {
            // Esta l�nea de Debug.Log te dir� si la explosi�n toc� una bomba
            Debug.Log("�Explosi�n toc� una bomba! Detonando...");

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