using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 1;
    [SerializeField] private DeathMenu deathMenu;               // arrástralo desde la escena
    [SerializeField] private Behaviour[] scriptsToDisableOnDeath; // ej: PlayerInputExample, PlayerBomb, PlayerIceWall

    private int hp;
    private bool dead;

    void Start()
    {
        hp = maxHP;
    }

    public void TakeDamage(int dmg = 1)
    {
        if (dead) return;
        hp -= dmg;
        if (hp <= 0) Die();
    }

    private void Die()
    {
        if (dead) return;
        dead = true;

        // Deshabilitar control
        if (scriptsToDisableOnDeath != null)
            foreach (var b in scriptsToDisableOnDeath)
                if (b) b.enabled = false;

        // Mostrar menú de muerte
        if (deathMenu) deathMenu.Show();
        else Time.timeScale = 0f; // fallback
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Explotion"))
            TakeDamage(999);
    }
}
