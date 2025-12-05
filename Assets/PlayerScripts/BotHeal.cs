using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHP = 1;
    [SerializeField] private float deathAnimationDuration = 0.6f; // Ajusta al largo de tu animación
    [SerializeField] private Behaviour[] scriptsToDisableOnDeath; // EnemyAI, etc.

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound; // Clip de audio para la muerte

    private Animator anim;
    private int hp;
    private bool dead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        hp = maxHP;
        EnemyManager.Instance.RegisterEnemy();

    }

    /// <summary>
    /// Inflige daño al enemigo
    /// </summary>
    public void TakeDamage(int dmg = 1)
    {
        if (dead) return;

        hp -= dmg;
        if (hp <= 0)
            Die();
    }

    /// <summary>
    /// Maneja la muerte del enemigo
    /// </summary>
    private void Die()
    {
        if (dead) return;
        dead = true;

        EnemyManager.Instance.EnemyDied(); // <<---- AGREGADO

        foreach (var s in scriptsToDisableOnDeath)
            if (s != null) s.enabled = false;

        if (anim != null)
            anim.SetTrigger("Death");

        if (deathSound != null)
        {
            AudioSource tempAudio = gameObject.AddComponent<AudioSource>();
            tempAudio.clip = deathSound;
            tempAudio.volume = 1f;
            tempAudio.spatialBlend = 0f;
            tempAudio.Play();
            Destroy(tempAudio, deathSound.length);
        }

        StartCoroutine(DisableAfterDeath());
    }

    private IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        if (anim != null)
            anim.enabled = false;

        Destroy(gameObject);
    }


}
