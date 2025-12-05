using UnityEngine;

public class PlayerButtonInput : MonoBehaviour
{
    public PlayerInputExample playerController; 

    public void MoveLeftButtonDown()
    {
        playerController.MoveLeft();
    }

    public void MoveLeftButtonUp()
    {
        playerController.StopMove();
    }



    public void MoveRightbuttonDown()
    {
        playerController.MoveRight();
    }

    public void MoveRightButtonUp()
    {
        playerController.StopMove();
    }



    public void MoveUpbuttonDown()
    {
        playerController.MoveUp();
    }

    public void MoveUpButtonUp()
    {
        playerController.StopMove();
    }



    public void MoveDownbuttonDown()
    {
        playerController.MoveDown();
    }

    public void MoveDownButtonUp()
    {
        playerController.StopMove();
    }





}
