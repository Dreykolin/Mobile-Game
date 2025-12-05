using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 1;
    [SerializeField] private DeathMenu deathMenu;
    [SerializeField] private Behaviour[] scriptsToDisableOnDeath;
    [SerializeField] private float deathAnimationDuration = 2.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound; // Sonido de muerte del jugador

    private Animator anim;
    private int hp;
    private bool dead;

    private Rigidbody2D rb;
    private Collider2D col;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        hp = maxHP;
    }

    public void TakeDamage(int dmg = 1)
    {
        if (dead) return;
        hp -= dmg;

        if (hp <= 0)
            Die();
    }

    private void Die()
    {
        if (dead) return;
        dead = true;

        Debug.Log("💀 Player Muerto");

        // 🔊 Reproducir sonido de muerte
        if (deathSound != null)
        {
            AudioSource tempAudio = gameObject.AddComponent<AudioSource>();
            tempAudio.clip = deathSound;
            tempAudio.volume = 1f;        // Volumen máximo
            tempAudio.spatialBlend = 0f;  // 2D, no afectado por distancia
            tempAudio.Play();
            Destroy(tempAudio, deathSound.length);
        }

        // 🔥 1. DESACTIVAR TODOS LOS SCRIPTS DE MOVIMIENTO/INPUT
        if (scriptsToDisableOnDeath != null)
        {
            foreach (var b in scriptsToDisableOnDeath)
                if (b) b.enabled = false;
        }

        // 🔥 2. ANIMACIÓN DE MUERTE
        if (anim)
            anim.SetTrigger("Death");

        // 🔥 3. DETENER COMPLETAMENTE FÍSICAS DEL PLAYER
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;  // desactiva físicas
        }

        if (col)
            col.enabled = false;  // evita que mobs te sigan empujando

        // 🔥 4. CONGELAR A TODOS LOS ENEMIGOS
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (var e in enemies)
        {
            e.FreezeWhenPlayerDies();
        }

        // 🔥 5. Mostrar menú después de la animación
        StartCoroutine(ShowDeathMenuAfterDelay());
    }

    private System.Collections.IEnumerator ShowDeathMenuAfterDelay()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        if (deathMenu)
            deathMenu.Show();
        else
            Time.timeScale = 0f;
    }

    // TRIGGERS:
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Explotion"))
        {
            TakeDamage(999);
        }
    }

    // COLISIONES:
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mobs"))
        {
            TakeDamage(999);
        }
    }
}
