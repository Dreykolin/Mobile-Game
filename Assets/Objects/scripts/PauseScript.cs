using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Asigna el Canvas del menú de pausa en el Inspector
    public GameObject pauseMenuCanvas;

    // <-- NUEVA VARIABLE -->
    // Asigna el Canvas del juego en el Inspector
    public GameObject gameCanvas;

    // Estado del juego
    public static bool isGamePaused = false;

    // Esta función ahora será llamada directamente por el botón táctil "Pause"
    public void TogglePause()
    {
        if (isGamePaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        pauseMenuCanvas.SetActive(true);
        gameCanvas.SetActive(false); // <-- NUEVA LÍNEA: Desactiva el GameCanvas
        Time.timeScale = 0f; // Pausa el juego
        isGamePaused = true;
    }

    public void Resume()
    {
        pauseMenuCanvas.SetActive(false);
        gameCanvas.SetActive(true); // <-- NUEVA LÍNEA: Activa el GameCanvas
        Time.timeScale = 1f; // Reanuda el juego
        isGamePaused = false;
    }

    public void OpenSettings()
    {
        Debug.Log("Abriendo opciones...");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
}