using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuCanvas;

    public GameObject gameCanvas;

    public static bool isGamePaused = false;

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
        gameCanvas.SetActive(false); 
        Time.timeScale = 0f; 
        isGamePaused = true;
    }

    public void Resume()
    {
        pauseMenuCanvas.SetActive(false);
        gameCanvas.SetActive(true); 
        Time.timeScale = 1f; 
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