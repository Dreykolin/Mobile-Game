using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Enemy Count")]
    public int totalEnemies;     // Se asigna al iniciar
    public int deadEnemies = 0;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemy()
    {
        totalEnemies++;
    }

    public void EnemyDied()
    {
        deadEnemies++;

        if (deadEnemies >= totalEnemies)
            GameWon();
    }

    private void GameWon()
    {
        Debug.Log("GANASTE LA PARTIDA!!!");

        // Aquí llamas a tu GameManager, UI, etc.
        GameManager.Instance.WinGame();
    }
}
