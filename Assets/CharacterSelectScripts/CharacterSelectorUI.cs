using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectorUI : MonoBehaviour
{
    [Header("Configuración de escenas")]
    [Tooltip("Índice o nombre de la escena de juego (si no se define, se carga la siguiente en el Build Settings).")]
    public string gameSceneName = "";

    // ---- OPCIONES DE PODER ----
    public void PlayWithoutPower()
    {
        PlayerPrefs.SetInt("SelectedPower", 0); // 0 = sin poder
        PlayerPrefs.Save();
        LoadGameScene();
    }

    public void PlayWithIceWall()
    {
        PlayerPrefs.SetInt("SelectedPower", 1); // 1 = IceWall
        PlayerPrefs.Save();
        LoadGameScene();
    }

    public void PlayWithFireBall()
    {
        PlayerPrefs.SetInt("SelectedPower", 2); // 2 = FireBall
        PlayerPrefs.Save();
        LoadGameScene();
    }

    // ---- CARGA DE ESCENA ----
    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
