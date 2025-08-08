using UnityEngine;

public class PlayerButtonInput : MonoBehaviour
{
    public PlayerInputExample playerController; // Referencia a tu script principal

    // Se llama cuando presionas el bot�n (mantener pulsado)
    public void MoveLeftButtonDown()
    {
        playerController.MoveLeft();
    }

    // Se llama cuando sueltas el bot�n
    public void MoveLeftButtonUp()
    {
        playerController.StopMove();
    }



    public void MoveRightbuttonDown()
    {
        playerController.MoveRight();
    }

    // Se llama cuando sueltas el bot�n
    public void MoveRightButtonUp()
    {
        playerController.StopMove();
    }



    public void MoveUpbuttonDown()
    {
        playerController.MoveUp();
    }

    // Se llama cuando sueltas el bot�n
    public void MoveUpButtonUp()
    {
        playerController.StopMove();
    }



    public void MoveDownbuttonDown()
    {
        playerController.MoveDown();
    }

    // Se llama cuando sueltas el bot�n
    public void MoveDownButtonUp()
    {
        playerController.StopMove();
    }





}
