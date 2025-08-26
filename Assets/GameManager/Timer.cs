using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private DeathMenu deathMenu; // ← arrástralo en el Inspector

    [Header("Timer")]
    [SerializeField] private float levelDuration = 180f;

    private float currentTime;
    private bool isTimerRunning = false;

    void Start()
    {
        currentTime = levelDuration;
        isTimerRunning = true;
        UpdateTimerUI();
    }

    void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.unscaledDeltaTime; // usa unscaled por si pausas en otro lado
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            GameOver();
        }
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        if (timerText) timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void GameOver()
    {
        Debug.Log("¡Tiempo agotado! El juego ha terminado.");
        if (deathMenu) deathMenu.Show();   // ← abre el menú de muerte/fin
        else Time.timeScale = 0f;          // fallback
    }
}
