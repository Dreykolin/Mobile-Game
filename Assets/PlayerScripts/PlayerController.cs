using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputExample : MonoBehaviour, PlayerControls.IPlayerActions
{
    private PlayerControls controls;
    private Vector2 moveInput = Vector2.zero;
    private bool isMoving = false;
    private Vector3 targetPosition;
    public float moveSpeed = 5f;

    // Referencia al PlayerBomb para saber cuál bomba ignorar
    private PlayerBomb playerBomb;

    // Variables para el movimiento continuo
    private Vector3 currentDirection = Vector3.zero;
    private float moveTimer = 0f;
    private const float moveInterval = 0.2f; // Intervalo de tiempo para cada paso

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
        moveInput = context.ReadValue<Vector2>();

        if (context.canceled)
        {
            StopMove();
        }
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
        // Lógica para el movimiento real hacia targetPosition
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;

                // Una vez que el paso termina, revisamos si debemos dar otro
                CheckForMovement();
            }
        }
        else
        {
            // Si el jugador no se está moviendo, revisamos el input del teclado
            // Esto es para que funcione el input del teclado/gamepad sin los botones táctiles
            if (moveInput != Vector2.zero)
            {
                if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                {
                    currentDirection.y = 0;
                    currentDirection.x = Mathf.Sign(moveInput.x);
                }
                else
                {
                    currentDirection.x = 0;
                    currentDirection.y = Mathf.Sign(moveInput.y);
                }
                MakeMove(currentDirection);
            }
        }
    }

    // Lógica principal de un solo paso de movimiento
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

    // Funciones que se activan con los botones táctiles
    // Al llamarlas, establecen la dirección y la lógica se encarga del resto
    public void MoveLeft()
    {
        currentDirection = Vector3.left;
        MakeMove(currentDirection);
    }

    public void MoveRight()
    {
        currentDirection = Vector3.right;
        MakeMove(currentDirection);
    }

    public void MoveUp()
    {
        currentDirection = Vector3.up;
        MakeMove(currentDirection);
    }

    public void MoveDown()
    {
        currentDirection = Vector3.down;
        MakeMove(currentDirection);
    }

    public void StopMove()
    {
        currentDirection = Vector3.zero;
        moveInput = Vector2.zero; // También reseteamos el input del teclado
    }

    // Método para revisar si debemos seguir moviéndonos
    private void CheckForMovement()
    {
        // Si hay una dirección definida, intentamos dar otro paso
        if (currentDirection != Vector3.zero)
        {
            MakeMove(currentDirection);
        }
    }


    // Este método se llama SOLO cuando el objeto está seleccionado en la escena y el script está activo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition, new Vector3(0.9f, 0.9f, 0));
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}