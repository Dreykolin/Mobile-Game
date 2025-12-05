using UnityEngine;
using UnityEngine.Tilemaps;

public class FireballProjectile : MonoBehaviour
{
    [Header("Configuración")]
    public float lifetime = 3f;

    // Referencias a los Tilemaps (busca automáticamente si no se asignan)
    private static Tilemap destructibleTilemap;
    private static Tilemap wallTilemap;
    public int damageAmount = 1; // 👈 Agregado para controlar cuánto daño hace
    private Rigidbody2D rb;

    // 💰 REFERENCIA PARA LA XP
    [Header("Audio")]
    public AudioClip fireballSound; // asignar en el inspector

    // 👇 AÑADIDO: Referencia al Animator
    private Animator anim;

    private PlayerStats playerStats;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 👇 AÑADIDO: Obtener el Animator (necesario si quieres efectos futuros)
        anim = GetComponent<Animator>();

        // Busca los Tilemaps una sola vez si no están asignados
        if (destructibleTilemap == null)
            destructibleTilemap = GameObject.Find("Tilemap_destructible")?.GetComponent<Tilemap>();

        if (wallTilemap == null)
            wallTilemap = GameObject.Find("Tilemap_blocks")?.GetComponent<Tilemap>();

        // 💰 BUSCAR AL JUGADOR PARA DARLE LA XP LUEGO
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerStats = playerObj.GetComponent<PlayerStats>();

        // ---------------------------------------------
        // 💡 LÓGICA ELIMINADA: Ya no se usa anim.SetBool/SetTrigger.
        // La animación debe ser el estado inicial del Animator.
        // ---------------------------------------------

        // 🔊 REPRODUCIR SONIDO AL INSTANCIARSE
        if (fireballSound != null)
        {
            // Tu lógica de audio temporal está bien, fuerza 2D y autodestrucción.
            AudioSource tempAudio = gameObject.AddComponent<AudioSource>();
            tempAudio.clip = fireballSound;
            tempAudio.volume = 1f;
            tempAudio.spatialBlend = 0f;
            tempAudio.Play();
            Destroy(tempAudio, fireballSound.length);
        }

        // Autodestrucción por tiempo (si no choca con nada)
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 direction, float speed)
    {
        if (rb != null)
            rb.linearVelocity = direction.normalized * speed;
    }

    // Método central para manejar la destrucción del proyectil
    private void DestroyProjectile()
    {
        // 💡 Si hubieran animaciones de "Impacto", se llamarían aquí con anim.SetTrigger
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 💀 MATAR MOBS
        if (other.CompareTag("Mobs"))
        {
            // Intentamos obtener el script de salud del enemigo
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                Debug.Log("🔥 ¡Mob impactado!");

                // ✅ AQUÍ ESTÁ EL CAMBIO: Llamamos a TakeDamage en lugar de Destroy
                enemyHealth.TakeDamage(damageAmount);

                // ⚠️ NOTA DE DISEÑO (XP):
                // Actualmente, esto dará XP cada vez que GOLPEES al enemigo.
                // Si el enemigo tiene 3 vidas, ganarás XP 3 veces.
                // Lo ideal es mover la lógica de "AddXP" dentro de la función Die() 
                // en el script EnemyHealth.
                if (playerStats != null) playerStats.AddXP(playerStats.xpPerKill);
            }
            else
            {
                // Fallback: Si el objeto tiene tag "Mobs" pero no tiene el script, lo destruimos a la fuerza
                Destroy(other.gameObject);
            }

            DestroyProjectile(); // La bola de fuego explota al impactar
            return;
        }

        // 2. ❄️ DERRETIR ICEWALL
        if (other.CompareTag("IceWall"))
        {
            Debug.Log("🔥 ¡Muro de hielo derretido!");
            Destroy(other.gameObject);
            DestroyProjectile();
            return;
        }

        // 3. 🤕 DAÑAR JUGADOR (PVP)
        if (other.CompareTag("Player"))
        {
            Debug.Log("🔥 ¡Jugador quemado!");
            other.GetComponent<PlayerHealth>()?.TakeDamage(1);
            DestroyProjectile();
            return;
        }

        // 5. 💣 TILEMAPS (Cajas destructibles y Paredes de la grilla)
        if (other.TryGetComponent<TilemapCollider2D>(out var tilemapCollider))
        {
            HandleTilemapCollision();
            DestroyProjectile();
            return;
        }

        // 6. CUALQUIER OTRA COSA SÓLIDA (Muros indestructibles, etc)
        if (!other.isTrigger)
        {
            DestroyProjectile();
        }
    }

    private void HandleTilemapCollision()
    {
        if (destructibleTilemap == null) return;

        // --- LÓGICA DE PRECISIÓN ---
        Vector3 hitPoint = transform.position;
        if (rb != null && rb.linearVelocity != Vector2.zero)
        {
            hitPoint += (Vector3)rb.linearVelocity.normalized * 0.5f;
        }

        Vector3Int cellPosition = destructibleTilemap.WorldToCell(hitPoint);

        // A. Intentar romper caja en la posición proyectada
        if (destructibleTilemap.HasTile(cellPosition))
        {
            destructibleTilemap.SetTile(cellPosition, null);
            if (playerStats != null) playerStats.AddXP(playerStats.xpPerBlock);
            return;
        }

        // B. Si chocó con pared indestructible del Tilemap
        if (wallTilemap != null && wallTilemap.HasTile(cellPosition))
        {
            return;
        }

        // C. Fallback (Por si acaso quedó entre dos tiles)
        Vector3Int[] neighbors = {
      new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
      new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0)
    };

        foreach (var offset in neighbors)
        {
            Vector3Int neighborPos = cellPosition + offset;
            if (destructibleTilemap.HasTile(neighborPos))
            {
                destructibleTilemap.SetTile(neighborPos, null);
                if (playerStats != null) playerStats.AddXP(playerStats.xpPerBlock);
                return;
            }
        }
    }
}

