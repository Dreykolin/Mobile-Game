using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Función para el botón "Jugar"
    public void PlayGame()
    {
        // Carga la siguiente escena en el orden de Build Settings.
        // Asegúrate de que tu escena de juego esté después de la de menú.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Debug.Log("Cargando juego...");
    }

    // Función para el botón "Opciones"
    public void OpenOptions()
    {
        // Por ahora, solo muestra un mensaje.
        // Aquí podrías cargar otro menú o panel de opciones.
        Debug.Log("Abriendo opciones...");
    }

    // Función para el botón "Salir"
    public void QuitGame()
    {
        // Cierra la aplicación (solo funciona en una build, no en el editor).
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}