using UnityEngine;

public class IceWall : MonoBehaviour
{
    [Header("Tiempo de vida")]
    public float lifeTime = 5f;

    // 👇 AÑADIDO: Clip de audio para el momento de la generación
    [Header("Audio")]
    public AudioClip spawnSound;

    private IWallOwner owner;

    private bool destroyed = false;

    void Start()
    {
        // 💡 DISPARAR EL SONIDO DE GENERACIÓN
        if (spawnSound != null)
        {
            // Reproduce el clip en la posición de la pared.
            // Crea un AudioSource temporal que se autodestruye.
            AudioSource.PlayClipAtPoint(spawnSound, transform.position);
        }

        Invoke(nameof(DestroyWall), lifeTime);
    }

    public void SetOwner(IWallOwner ownerScript)
    {
        owner = ownerScript;
    }

    void DestroyWall()
    {
        if (destroyed) return;
        destroyed = true;


        owner?.NotifyWallDestroyed(GetComponent<Collider2D>());

        Destroy(gameObject);
    }
}