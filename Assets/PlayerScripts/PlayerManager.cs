using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Poderes del jugador")]
    public PlayerIceWall iceWall;
    public PlayerFireBall fireBall;

    private string activePower = "None";

    void Start()
    {
        // Desactiva ambos poderes al iniciar
        if (iceWall != null) iceWall.enabled = false;
        if (fireBall != null) fireBall.enabled = false;

        // Carga la preferencia guardada: 0 = sin poder, 1 = IceWall, 2 = FireBall
        int selectedPower = PlayerPrefs.GetInt("SelectedPower", 0);

        if (selectedPower == 1 && iceWall != null)
        {
            iceWall.enabled = true;
            activePower = "IceWall";
            Debug.Log("🧊 Modo con poder: IceWall activado");
        }
        else if (selectedPower == 2 && fireBall != null)
        {
            fireBall.enabled = true;
            activePower = "FireBall";
            Debug.Log("🔥 Modo con poder: FireBall activado");
        }
        else
        {
            Debug.Log("🚶 Modo sin poder: solo movimiento");
        }
    }

    // ----- SELECCIÓN DE PODER -----
    public void SelectIceWall()
    {
        PlayerPrefs.SetInt("SelectedPower", 1);
        PlayerPrefs.Save();
        Debug.Log("🧊 Poder guardado: IceWall");
    }

    public void SelectFireBall()
    {
        PlayerPrefs.SetInt("SelectedPower", 2);
        PlayerPrefs.Save();
        Debug.Log("🔥 Poder guardado: FireBall");
    }

    // ----- DESACTIVAR TODO -----
    private void DisableAllPowers()
    {
        if (iceWall != null) iceWall.enabled = false;
        if (fireBall != null) fireBall.enabled = false;
        activePower = "None";
    }

    // ----- USO DEL PODER (opcional, si quieres un botón genérico de acción) -----
    public void UseActivePower()
    {
        if (activePower == "IceWall" && iceWall != null && iceWall.enabled)
        {
            iceWall.putWall();
        }
        // FireBall ya se maneja automáticamente con su joystick
    }
}
