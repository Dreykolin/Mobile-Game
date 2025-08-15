using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputExample : MonoBehaviour, PlayerControls.IPlayerActions
{
    private PlayerControls controls;
    private bool isMoving = false;
    private Vector3 targetPosition;
    public float moveSpeed = 5f;

    private PlayerBomb playerBomb;

    private Vector3 currentDirection = Vector3.zero;
    private Vector2 keyboardMoveInput = Vector2.zero;

    [Header("UI Controls")]
    public Joystick joystick;

    void Awake()
    {
        controls = new PlayerControls();
        controls.player.SetCallbacks(this);
        targetPosition = transform.position;

        playerBomb = GetComponent<PlayerBomb>();
        if (playerBomb == null)
            Debug.LogWarning("PlayerInputExample: No se encontró PlayerBomb en el mismo GameObject.");
    }

    void OnEnable() => controls.player.Enable();
    void OnDisable() => controls.player.Disable();

    public void OnMove(InputAction.CallbackContext context)
    {
        keyboardMoveInput = context.ReadValue<Vector2>();
    }

    public void OnFight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Atacando!");
        }
    }

    void Update()
    {
        // Movimiento suave hacia el objetivo
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }

        // Lógica para determinar la dirección de movimiento
        Vector2 inputVector = Vector2.zero;
        if (joystick != null && joystick.input.sqrMagnitude > 0.1f)
        {
            inputVector = joystick.input;
        }
        else if (keyboardMoveInput.sqrMagnitude > 0.1f)
        {
            inputVector = keyboardMoveInput;
        }

        // Si hay input, actualizamos la dirección cardinal
        if (inputVector.sqrMagnitude > 0.0001f)
        {
            if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y))
            {
                currentDirection = new Vector3(Mathf.Sign(inputVector.x), 0, 0);
            }
            else
            {
                currentDirection = new Vector3(0, Mathf.Sign(inputVector.y), 0);
            }
        }
        else
        {
            currentDirection = Vector3.zero;
        }

        // === ¡El cambio clave está aquí! ===
        // Si no se está moviendo y hay una dirección, intenta dar un paso.
        if (!isMoving && currentDirection != Vector3.zero)
        {
            MakeMove(currentDirection);
        }
    }

    private void MakeMove(Vector3 direction)
    {
        if (isMoving) return;

        Vector3 newPos = targetPosition + direction;
        Vector2 boxSize = new Vector2(0.9f, 0.9f);
        Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)newPos, boxSize, 0f);
        bool blocked = false;

        if (hits != null)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;
                if (hit.CompareTag("blocks") || hit.CompareTag("destructible"))
                {
                    blocked = true;
                    break;
                }
                if (hit.CompareTag("Bomb"))
                {
                    bool esMiBomba = (playerBomb != null && playerBomb.EsUltimaBomba(hit));
                    if (!esMiBomba)
                    {
                        blocked = true;
                        break;
                    }
                }
            }
        }

        if (!blocked)
        {
            targetPosition = newPos;
            isMoving = true;
        }
    }

    // ... Las funciones MoveX y StopMove ya no son necesarias con esta lógica.

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition, new Vector3(0.9f, 0.9f, 0));
    }

    public bool IsMoving()
    {
        return isMoving;
    }

// Funciones que se activan con los botones táctiles
public void MoveLeft()
    {
        currentDirection = Vector3.left;
        if (!isMoving) MakeMove(currentDirection);
    }

    public void MoveRight()
    {
        currentDirection = Vector3.right;
        if (!isMoving) MakeMove(currentDirection);
    }

    public void MoveUp()
    {
        currentDirection = Vector3.up;
        if (!isMoving) MakeMove(currentDirection);
    }

    public void MoveDown()
    {
        currentDirection = Vector3.down;
        if (!isMoving) MakeMove(currentDirection);
    }

    public void StopMove()
    {
        currentDirection = Vector3.zero;
    }
}