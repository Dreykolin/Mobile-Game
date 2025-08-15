using UnityEngine;
using TMPro; // Necesitas este namespace para usar TextMesh Pro

public class GameManager : MonoBehaviour
{
    // Asigna este objeto de texto en el Inspector de Unity
    public TextMeshProUGUI timerText;

    // La duración del nivel en segundos
    public float levelDuration = 180f; // 3 minutos

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
        if (isTimerRunning)
        {
            // Reduce el tiempo cada segundo
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                isTimerRunning = false;
                GameOver();
            }

            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {   
        // Convierte los segundos a un formato de minutos:segundos
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        // Actualiza el texto en la UI
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void GameOver()
    {
        Debug.Log("¡Tiempo agotado! El juego ha terminado.");
        // Aquí puedes añadir la lógica para terminar el juego,
        // como mostrar un panel de "Game Over".
    }
}