using UnityEngine;

public class PlayerButtonInput : MonoBehaviour
{
    public PlayerInputExample playerController; // Referencia a tu script principal

    // Se llama cuando presionas el botón (mantener pulsado)
    public void MoveLeftButtonDown()
    {
        playerController.MoveLeft();
    }

    // Se llama cuando sueltas el botón
    public void MoveLeftButtonUp()
    {
        playerController.StopMove();
    }



    public void MoveRightbuttonDown()
    {
        playerController.MoveRight();
    }

    // Se llama cuando sueltas el botón
    public void MoveRightButtonUp()
    {
        playerController.StopMove();
    }



    public void MoveUpbuttonDown()
    {
        playerController.MoveUp();
    }

    // Se llama cuando sueltas el botón
    public void MoveUpButtonUp()
    {
        playerController.StopMove();
    }



    public void MoveDownbuttonDown()
    {
        playerController.MoveDown();
    }

    // Se llama cuando sueltas el botón
    public void MoveDownButtonUp()
    {
        playerController.StopMove();
    }





}
