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
        if (context.performed && !isMoving)
        {
            moveInput = context.ReadValue<Vector2>();

            if (moveInput != Vector2.zero)
            {
                // Restringir a 4 direcciones
                if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                {
                    moveInput.y = 0;
                    moveInput.x = Mathf.Sign(moveInput.x);
                }
                else
                {
                    moveInput.x = 0;
                    moveInput.y = Mathf.Sign(moveInput.y);
                }

                Vector3 direction = new Vector3(moveInput.x, moveInput.y, 0);
                Vector3 newPos = targetPosition + direction;

                Vector2 boxSize = new Vector2(0.9f, 0.9f);

                // Obtenemos todos los colliders en esa casilla (sin filtrar por layer para que no falle si tu bomb está en otra layer)
                Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)newPos, boxSize, 0f);

                bool blocked = false;

                if (hits != null && hits.Length > 0)
                {
                    foreach (Collider2D hit in hits)
                    {
                        if (hit == null) continue;

                        // Bloque real: muros o bloques destructibles (asegúrate de tener estas tags)
                        if (hit.CompareTag("blocks") || hit.CompareTag("destructible"))
                        {
                            blocked = true;
                            break;
                        }

                        // Si es bomba, permitir solo si es la última bomba propia
                        if (hit.CompareTag("Bomb"))
                        {
                            bool esMiBomba = (playerBomb != null && playerBomb.EsUltimaBomba(hit));
                            if (esMiBomba)
                            {
                                // ignorar este collider y seguir chequeando otros colliders
                                continue;
                            }
                            else
                            {
                                blocked = true;
                                break;
                            }
                        }

                        // Otros colliders (personajes, items): si no quieres que bloqueen, ignóralos.
                        // Si quieres que otros tipos bloqueen, añade sus tags aquí.
                    }
                }

                if (!blocked)
                {
                    targetPosition = newPos;
                    isMoving = true;
                }
                else
                {
                    Debug.Log("Movimiento bloqueado (por collider en la casilla).");
                }
            }
        }
        else if (context.canceled)
        {
            moveInput = Vector2.zero;
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
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }







    public void MoveLeft()
    {
        if (!isMoving)
        {
            Vector3 direction = Vector3.left;  // Izquierda
            Vector3 newPos = targetPosition + direction;

            Vector2 boxSize = new Vector2(0.9f, 0.9f);
            int layerMask = LayerMask.GetMask("Default");
            Collider2D hit = Physics2D.OverlapBox(newPos, boxSize, 0f, layerMask);

            if (hit == null || !hit.CompareTag("blocks"))
            {
                targetPosition = newPos;
                isMoving = true;
            }
        }
    }

    public void MoveRight()
    {
        if (!isMoving)
        {
            Vector3 direction = Vector3.right;  // Derecha
            Vector3 newPos = targetPosition + direction;

            Vector2 boxSize = new Vector2(0.9f, 0.9f);
            int layerMask = LayerMask.GetMask("Default");
            Collider2D hit = Physics2D.OverlapBox(newPos, boxSize, 0f, layerMask);

            if (hit == null || !hit.CompareTag("blocks"))
            {
                targetPosition = newPos;
                isMoving = true;
            }
        }
    }

    public void MoveUp()
    {
        if (!isMoving)
        {
            Vector3 direction = Vector3.up;  // Arriba
            Vector3 newPos = targetPosition + direction;

            Vector2 boxSize = new Vector2(0.9f, 0.9f);
            int layerMask = LayerMask.GetMask("Default");
            Collider2D hit = Physics2D.OverlapBox(newPos, boxSize, 0f, layerMask);

            if (hit == null || !hit.CompareTag("blocks"))
            {
                targetPosition = newPos;
                isMoving = true;
            }
        }
    }

    public void MoveDown()
    {
        if (!isMoving)
        {
            Vector3 direction = Vector3.down;  // Abajo
            Vector3 newPos = targetPosition + direction;

            Vector2 boxSize = new Vector2(0.9f, 0.9f);
            int layerMask = LayerMask.GetMask("Default");
            Collider2D hit = Physics2D.OverlapBox(newPos, boxSize, 0f, layerMask);

            if (hit == null || !hit.CompareTag("blocks"))
            {
                targetPosition = newPos;
                isMoving = true;
            }
        }
    }

    public void StopMove()
    {
        // Puedes dejarlo vacío o agregar lógica para detener animaciones o resetear estados.
    }


    // Este método se llama SOLO cuando el objeto está seleccionado en la escena y el script está activo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition, new Vector3(0.9f, 0.9f, 0));
    }
}