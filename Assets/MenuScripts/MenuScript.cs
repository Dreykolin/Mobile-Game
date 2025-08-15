using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Funci�n para el bot�n "Jugar"
    public void PlayGame()
    {
        // Carga la siguiente escena en el orden de Build Settings.
        // Aseg�rate de que tu escena de juego est� despu�s de la de men�.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Debug.Log("Cargando juego...");
    }

    // Funci�n para el bot�n "Opciones"
    public void OpenOptions()
    {
        // Por ahora, solo muestra un mensaje.
        // Aqu� podr�as cargar otro men� o panel de opciones.
        Debug.Log("Abriendo opciones...");
    }

    // Funci�n para el bot�n "Salir"
    public void QuitGame()
    {
        // Cierra la aplicaci�n (solo funciona en una build, no en el editor).
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}