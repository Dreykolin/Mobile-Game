using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBomb : MonoBehaviour, PlayerControls.IPlayerActions
{
    public GameObject bombPrefab;
    public float bombCooldown = 2f;
    private float nextBombTime = 0f;

    private PlayerControls controls;

    // Referencia al collider de la última bomba puesta por este jugador
    private Collider2D lastBombCollider;
    private bool stillOnLastBomb = false;

    private Collider2D playerCollider;

    void Awake()
    {
        controls = new PlayerControls();
        controls.player.SetCallbacks(this);
        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
            Debug.LogWarning("PlayerBomb: No se encontró Collider2D en el jugador.");
    }

    void OnEnable() => controls.player.Enable();
    void OnDisable() => controls.player.Disable();

    public void OnFight(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time >= nextBombTime)
        {
            PlaceBomb();
            nextBombTime = Time.time + bombCooldown;
        }
    }

    public void OnMove(InputAction.CallbackContext context) { }

    private void PlaceBomb()
    {
        GameObject bombObj = Instantiate(bombPrefab, transform.position, Quaternion.identity);

        // Intentamos obtener el Collider2D (en caso que esté en un hijo)
        Collider2D col = bombObj.GetComponent<Collider2D>();
        if (col == null) col = bombObj.GetComponentInChildren<Collider2D>();

        lastBombCollider = col;

        if (playerCollider != null && lastBombCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, lastBombCollider, true);
            stillOnLastBomb = true;
        }
        else
        {
            Debug.LogWarning("PlayerBomb: No se pudo ignorar colisión (playerCollider o bomb collider null).");
        }
    }

    void Update()
    {
        // Si estamos marcando que seguimos sobre la bomba, revisamos si ya salimos de la casilla (comprobación por grid)
        if (stillOnLastBomb && lastBombCollider != null)
        {
            Vector2Int playerGrid = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            Vector2Int bombGrid = new Vector2Int(Mathf.RoundToInt(lastBombCollider.transform.position.x), Mathf.RoundToInt(lastBombCollider.transform.position.y));

            if (playerGrid != bombGrid)
            {
                // Re-habilitar la colisión física
                Physics2D.IgnoreCollision(playerCollider, lastBombCollider, false);
                stillOnLastBomb = false;
                lastBombCollider = null;
            }
        }
    }

    // Llamar si la bomba explota/destruye antes de que el jugador se vaya
    public void ClearLastBombReference(Collider2D bombCollider)
    {
        if (lastBombCollider == bombCollider)
        {
            // Aseguramos intentar re-habilitar (por si)
            if (playerCollider != null && lastBombCollider != null)
                Physics2D.IgnoreCollision(playerCollider, lastBombCollider, false);

            lastBombCollider = null;
            stillOnLastBomb = false;
        }
    }

    // Método que pregunta si un collider corresponde a la última bomba (robusto)
    public bool EsUltimaBomba(Collider2D col)
    {
        if (!stillOnLastBomb || lastBombCollider == null || col == null) return false;

        // Igualdad directa de collider
        if (col == lastBombCollider) return true;

        // Mismo gameObject
        if (col.gameObject == lastBombCollider.gameObject) return true;

        // Si ambos están unidos a un rigidbody, comparar el GameObject del rigidbody
        if (col.attachedRigidbody != null && lastBombCollider.attachedRigidbody != null)
        {
            if (col.attachedRigidbody.gameObject == lastBombCollider.attachedRigidbody.gameObject)
                return true;
        }

        return false;
    }

    // Método público que puedes llamar desde otros (ej: si la bomba se destruye)
    public void NotifyBombDestroyed(Collider2D bombCollider)
    {
        ClearLastBombReference(bombCollider);
    }

    public void putBomb()
    {
        if (Time.time >= nextBombTime)
        {
            PlaceBomb();
            nextBombTime = Time.time + bombCooldown;
        }
    }
}