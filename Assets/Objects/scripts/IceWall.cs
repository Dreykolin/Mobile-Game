using UnityEngine;

public class IceWall : MonoBehaviour
{
    [Header("Tiempo de vida")]
    public float lifeTime = 5f; // se destruye después de 5 segundos

    private IWallOwner owner;

    private bool destroyed = false;

    void Start()
    {
        // Programa la destrucción automática
        Invoke(nameof(DestroyWall), lifeTime);
    }

    // Permite que el dueño (IceHeroPower) registre que esta pared le pertenece
    public void SetOwner(IWallOwner ownerScript)
    {
        owner = ownerScript;
    }

    void DestroyWall()
    {
        if (destroyed) return;
        destroyed = true;

        // Aquí podrías poner algún efecto visual de hielo rompiéndose
        // por ejemplo: Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);

        // Avisar al dueño que la pared se destruyó
        owner?.NotifyWallDestroyed(GetComponent<Collider2D>());

        // Destruir la pared
        Destroy(gameObject);
    }

}
