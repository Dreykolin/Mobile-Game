using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputExample : MonoBehaviour, PlayerControls.IPlayerActions
{
    private PlayerControls controls;

    // MOVIMIENTO POR CASILLA
    private bool isMoving = false;
    private Vector3 targetPosition;
    public float moveSpeed = 5f;

    private PlayerBomb playerBomb;
    private PlayerIceWall icePower;

    private Vector3 currentDirection = Vector3.zero;
    private Vector2 keyboardMoveInput = Vector2.zero;

    [Header("UI Controls")]
    public Joystick joystick;

    // ---------- ANIMACIÓN ----------
    private Animator anim;
    private Vector2 lastLookDir = Vector2.down; // por defecto mira abajo
    [Header("Audio")]
    public AudioClip walkSound; // El sonido del paso que arrastrarás desde el Inspector
    private AudioSource audioSource;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) // Asegurarse de que el componente existe
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = walkSound;
        audioSource.playOnAwake = false; // Queremos reproducirlo con código
        controls = new PlayerControls();
        controls.player.SetCallbacks(this);
        targetPosition = transform.position;

        anim = GetComponent<Animator>();

        playerBomb = GetComponent<PlayerBomb>();
        icePower = GetComponent<PlayerIceWall>();
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
            Debug.Log("Atacando!");
    }
    private float stepInterval = 0.3f; // tiempo entre pasos
    private float stepTimer = 0f;

    void Update()
    {
        // ------------------------------------------------
        // MOVER HACIA targetPosition
        // ------------------------------------------------
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            float snap = 1f / 32f;
            Vector3 pos = transform.position;
            pos.x = Mathf.Round(pos.x / snap) * snap;
            pos.y = Mathf.Round(pos.y / snap) * snap;
            transform.position = pos;

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }

            // -------------------------------
            // REPRODUCIR SONIDO DE PASOS
            // -------------------------------
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                if (walkSound != null)
                {
                    audioSource.Play();
                    stepTimer = stepInterval;
                }
            }
        }
        else
        {
            // Reiniciar timer cuando no se mueve
            stepTimer = 0f;
        }

        // ------------------------------------------------
        // ELEGIR DIRECCIÓN (teclado o joystick)
        // ------------------------------------------------
        Vector2 inputVector = Vector2.zero;

        if (joystick != null && joystick.input.sqrMagnitude > 0.1f)
            inputVector = joystick.input;
        else if (keyboardMoveInput.sqrMagnitude > 0.1f)
            inputVector = keyboardMoveInput;

        if (!isMoving)
        {
            if (inputVector.sqrMagnitude > 0.0001f)
            {
                if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y))
                    currentDirection = new Vector3(Mathf.Sign(inputVector.x), 0, 0);
                else
                    currentDirection = new Vector3(0, Mathf.Sign(inputVector.y), 0);

                lastLookDir = currentDirection;
            }
            else
            {
                currentDirection = Vector3.zero;
            }
        }

        // ------------------------------------------------
        // ACTUALIZAR PARÁMETROS DEL ANIMATOR
        // ------------------------------------------------
        anim.SetFloat("MoveX", lastLookDir.x);
        anim.SetFloat("MoveY", lastLookDir.y);
        anim.SetBool("IsMoving", isMoving);

        // ------------------------------------------------
        // INICIAR MOVIMIENTO SI NO SE ESTÁ MOVIENDO
        // ------------------------------------------------
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

        foreach (Collider2D hit in hits)
        {
            if (!hit) continue;

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

            if (hit.CompareTag("IceWall"))
            {
                blocked = true;
                break;
            }
        }

        if (!blocked)
        {
            targetPosition = newPos;
            isMoving = true;

            // ❌ LÍNEA ELIMINADA: Ya no llamamos anim.SetBool("IsMoving", true) aquí.
            // Lo hace la línea centralizada al final de Update.
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition, new Vector3(0.9f, 0.9f, 0));
    }

    public bool IsMoving() => isMoving;

    // JOYSTICK BUTTONS
    public void MoveLeft() { currentDirection = Vector3.left; if (!isMoving) MakeMove(currentDirection); }
    public void MoveRight() { currentDirection = Vector3.right; if (!isMoving) MakeMove(currentDirection); }
    public void MoveUp() { currentDirection = Vector3.up; if (!isMoving) MakeMove(currentDirection); }
    public void MoveDown() { currentDirection = Vector3.down; if (!isMoving) MakeMove(currentDirection); }

    public void StopMove()
    {
        currentDirection = Vector3.zero;
    }

    public void ForceSnapTo(Vector3 worldPos)
    {
        isMoving = false;
        targetPosition = worldPos;
        transform.position = worldPos;
        Physics2D.SyncTransforms();
    }
}
