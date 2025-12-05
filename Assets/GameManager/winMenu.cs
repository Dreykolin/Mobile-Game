using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinMenu : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject gameCanvas;       // HUD / gameplay UI
    [SerializeField] private GameObject pauseMenuCanvas;  // opcional
    [SerializeField] private GameObject winMenuCanvas;    // canvas del menú de victoria

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private TextMeshProUGUI bestTimeText;

    private bool shown = false;

    void Awake()
    {
        if (winMenuCanvas) winMenuCanvas.SetActive(false);
    }

    /// <summary>
    /// Muestra el menú de victoria con tiempos.
    /// </summary>
    public void Show(float finalTime, float bestTime)
    {
        if (shown) return;
        shown = true;

        // Apagar otros canvases
        if (gameCanvas) gameCanvas.SetActive(false);
        if (pauseMenuCanvas) pauseMenuCanvas.SetActive(false);

        // Activar el WIN MENU
        if (winMenuCanvas) winMenuCanvas.SetActive(true);

        // Mostrar tiempos
        if (finalTimeText) finalTimeText.text = "Tiempo: " + Format(finalTime);
        if (bestTimeText) bestTimeText.text = "Record: " + Format(bestTime);

        // Pausar juego
        Time.timeScale = 0f;
    }

    private string Format(float t)
    {
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        return $"{m:00}:{s:00}";
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
        SceneManager.LoadScene(0);
    }

    // Botón: Salir
    public void OnQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
