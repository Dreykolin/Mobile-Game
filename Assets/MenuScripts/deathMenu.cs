using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject gameCanvas;      // HUD / gameplay UI
    [SerializeField] private GameObject pauseMenuCanvas; // (opcional) si existe
    [SerializeField] private GameObject deathMenuCanvas; // menú de muerte

    private bool shown = false;

    void Awake()
    {
        if (deathMenuCanvas) deathMenuCanvas.SetActive(false);
    }

    public void Show()
    {
        if (shown) return;
        shown = true;

        // Apaga todo lo demás
        if (gameCanvas) gameCanvas.SetActive(false);
        if (pauseMenuCanvas) pauseMenuCanvas.SetActive(false);

        // Muestra el menú de muerte
        if (deathMenuCanvas) deathMenuCanvas.SetActive(true);

        // Pausa el juego
        Time.timeScale = 0f;
    }

    // Botón: Reintentar
    public void OnRetry()
    {
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // Botón: Menú principal
    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // o "MainMenu"
    }

    // Botón: Salir
    public void OnQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
