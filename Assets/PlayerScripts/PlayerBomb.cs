using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBomb : MonoBehaviour, PlayerControls.IPlayerActions, IBombOwner
{
    public GameObject bombPrefab;
    public int maxBombs = 1; // cantidad máxima de bombas que puede poner
    private int bombsAvailable;

    private PlayerControls controls;

    private Collider2D lastBombCollider;
    private bool stillOnLastBomb = false;

    private Collider2D playerCollider;
    private PlayerInputExample playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInputExample>();
        if (playerInput == null)
            Debug.LogWarning("No se encontró PlayerInputExample en el mismo GameObject");

        controls = new PlayerControls();
        controls.player.SetCallbacks(this);

        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
            Debug.LogWarning("PlayerBomb: No se encontró Collider2D en el jugador.");

        bombsAvailable = maxBombs; // inicializamos
    }

    void OnEnable() => controls.player.Enable();
    void OnDisable() => controls.player.Disable();

    public void OnFight(InputAction.CallbackContext context)
    {
        if (context.performed && !playerInput.IsMoving())
        {
            PlaceBomb();
        }
    }

    public void OnMove(InputAction.CallbackContext context) { }

    private void PlaceBomb()
    {
        if (bombsAvailable <= 0) return;

        GameObject bombObj = Instantiate(bombPrefab, transform.position, Quaternion.identity);

        Collider2D col = bombObj.GetComponent<Collider2D>();
        if (col == null) col = bombObj.GetComponentInChildren<Collider2D>();

        lastBombCollider = col;

        if (playerCollider != null && lastBombCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, lastBombCollider, true);
            stillOnLastBomb = true;
        }

        bombsAvailable--;

        // Asignamos dueño usando la interfaz IBombOwner
        Bomb bombScript = bombObj.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.SetOwner(this);
        }
    }

    void Update()
    {
        // Manejar la última bomba
        if (stillOnLastBomb && lastBombCollider != null)
        {
            float distance = Vector2.Distance(transform.position, lastBombCollider.transform.position);
            if (distance > 0.6f)
            {
                Physics2D.IgnoreCollision(playerCollider, lastBombCollider, false);
                stillOnLastBomb = false;
                lastBombCollider = null;
            }
        }

        // Ignorar bombas debajo del jugador
        CheckAndIgnoreBombsUnderPlayer();
    }

    void CheckAndIgnoreBombsUnderPlayer()
    {
        Vector2 pos = transform.position;
        Vector2 boxSize = new Vector2(0.9f, 0.9f);

        Collider2D[] bombsUnderPlayer = Physics2D.OverlapBoxAll(pos, boxSize, 0f);
        foreach (var bombCollider in bombsUnderPlayer)
        {
            if (bombCollider != null && bombCollider.CompareTag("Bomb"))
            {
                Physics2D.IgnoreCollision(playerCollider, bombCollider, true);
            }
        }
    }

    public void RecoverBomb()
    {
        bombsAvailable++;
        if (bombsAvailable > maxBombs) bombsAvailable = maxBombs;
    }

    public void ClearLastBombReference(Collider2D bombCollider)
    {
        if (lastBombCollider == bombCollider)
        {
            if (playerCollider != null)
                Physics2D.IgnoreCollision(playerCollider, lastBombCollider, false);

            lastBombCollider = null;
            stillOnLastBomb = false;
        }
    }

    public bool EsUltimaBomba(Collider2D col)
    {
        if (!stillOnLastBomb || lastBombCollider == null || col == null) return false;
        if (col == lastBombCollider || col.gameObject == lastBombCollider.gameObject) return true;
        if (col.attachedRigidbody != null && lastBombCollider.attachedRigidbody != null)
            return col.attachedRigidbody.gameObject == lastBombCollider.attachedRigidbody.gameObject;
        return false;
    }

    // 🔹 Implementación de IBombOwner
    public void NotifyBombDestroyed(Collider2D bombCollider)
    {
        ClearLastBombReference(bombCollider);
        RecoverBomb(); // recarga de bomba automática
    }

    public void putBomb()
    {
        if (!playerInput.IsMoving())
            PlaceBomb();
    }
}
