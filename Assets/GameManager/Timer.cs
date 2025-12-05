using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private DeathMenu deathMenu;
    [SerializeField] private WinMenu winMenu;


    [Header("Timer")]
    [SerializeField] private float levelDuration = 180f;

    private float currentTime;
    private bool isTimerRunning = false;

    // ---- ENEMY COUNTER ----
    [Header("Enemy Count")]
    public int totalEnemies = 0;
    public int deadEnemies = 0;

    private bool gameEnded = false;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentTime = levelDuration;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    void Update()
    {
        if (!isTimerRunning || gameEnded) return;

        currentTime -= Time.unscaledDeltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            GameOver();
        }

        UpdateTimerUI();
    }

    // ============================================================
    //                       ENEMY MANAGEMENT
    // ============================================================

    public void RegisterEnemy()
    {
        totalEnemies++;
    }

    public void EnemyDied()
    {
        deadEnemies++;

        if (deadEnemies >= totalEnemies)
            WinGame();
    }

    // ============================================================
    //                       GAME ENDINGS
    // ============================================================

    void GameOver()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("¡Tiempo agotado! El juego ha terminado.");

        if (deathMenu) deathMenu.Show();
        else Time.timeScale = 0f;
    }

    public void WinGame()
{
    if (gameEnded) return;
    gameEnded = true;

    float timeUsed = levelDuration - currentTime;
    RecordManager.AddRecord(timeUsed);
    float best = RecordManager.GetBestRecord();
        Debug.Log("GANASTE! Tiempo: " + timeUsed + " Best: " + best);

    Time.timeScale = 0f;

    if (winMenu) winMenu.Show(timeUsed, best);
}

    // ============================================================
    //                       BEST TIME
    // ============================================================

    public void SaveBestTime(float time)
    {
        float best = PlayerPrefs.GetFloat("BestTime", float.MaxValue);

        if (time < best)
        {
            PlayerPrefs.SetFloat("BestTime", time);
            PlayerPrefs.Save();
        }
    }

    public float GetBestTime()
    {
        return PlayerPrefs.GetFloat("BestTime", float.MaxValue);
    }

    // ============================================================

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        if (timerText) timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
