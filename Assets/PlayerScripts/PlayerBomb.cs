using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerBomb : MonoBehaviour, PlayerControls.IPlayerActions, IBombOwner
{
    [Header("Bombas")]
    public GameObject bombPrefab;
    public int maxBombs = 1;
    private int bombsAvailable;

    [Header("Grid / Tilemap")]
    [SerializeField] private Tilemap gridTilemap;

    // <-- NUEVA VARIABLE -->
    [Header("UI")]
    public BombCounterUI bombCounterUI;

    private PlayerControls controls;

    private Collider2D lastBombCollider;
    private bool stillOnLastBomb = false;
    private Collider2D playerCollider;

    private PlayerInputExample playerInput;
    private Vector2Int lastCardinalDir = Vector2Int.down;
    private Vector3Int lastCell;
    private Vector3Int prevCell;

    private bool bombsLocked = false;

    public void SetBombsLocked(bool v)
    {
        bombsLocked = v;
    }

    void Awake()
    {
        playerInput = GetComponent<PlayerInputExample>();
        controls = new PlayerControls();
        controls.player.SetCallbacks(this);
        playerCollider = GetComponent<Collider2D>();
        bombsAvailable = maxBombs;
    }

    void Start()
    {
        if (gridTilemap != null)
        {
            lastCell = gridTilemap.WorldToCell(transform.position);
            prevCell = lastCell;
        }

     
        if (bombCounterUI != null)
        {
            bombCounterUI.Setup(maxBombs);
        }
    }

    void OnEnable() => controls.player.Enable();
    void OnDisable() => controls.player.Disable();

    public void OnFight(InputAction.CallbackContext context)
    {
        if (context.performed)
            PlaceBomb();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 v = context.ReadValue<Vector2>();
        if (v.sqrMagnitude > 0.0001f)
        {
            if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
                lastCardinalDir = new Vector2Int(v.x > 0 ? 1 : -1, 0);
            else
                lastCardinalDir = new Vector2Int(0, v.y > 0 ? 1 : -1);
        }
    }

    public void SetMaxBombs(int n)
    {
        maxBombs = Mathf.Max(1, n);
        if (bombsAvailable > maxBombs)
            bombsAvailable = maxBombs;
    }
    void Update()
    {
        if (gridTilemap != null)
        {
            var currentCell = gridTilemap.WorldToCell(transform.position);
            if (currentCell != lastCell)
            {
                prevCell = lastCell;
                lastCell = currentCell;
            }
        }
       /*El jugador pueda salir de su propia bomba recién puesta,Y una vez que se aleje, vuelva a colisionar solo con esa bomba.
         * 
         * 
         */
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

        CheckAndIgnoreBombsUnderPlayer();
    }

    private void PlaceBomb()
    {
        if (bombsAvailable <= 0) return;

        Vector3 spawnPos;

        if (gridTilemap != null)
        {
            bool isMoving = playerInput != null && playerInput.IsMoving();
            Vector3Int bombCell = isMoving
                ? prevCell + new Vector3Int(lastCardinalDir.x, lastCardinalDir.y, 0)
                : lastCell;

            spawnPos = gridTilemap.CellToWorld(bombCell) + new Vector3(0.5f, 0.5f, 0f);
        }
        else
        {
            Vector3 p = transform.position;
            p = new Vector3(Mathf.Floor(p.x) + 0.5f, Mathf.Floor(p.y) + 0.5f, 0);
            spawnPos = p;
        }

        GameObject bombObj = Instantiate(bombPrefab, spawnPos, Quaternion.identity);

        Collider2D col = bombObj.GetComponent<Collider2D>();
        if (col == null) col = bombObj.GetComponentInChildren<Collider2D>();
        lastBombCollider = col;

        if (playerCollider != null && lastBombCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, lastBombCollider, true);
            stillOnLastBomb = true;
        }

        bombsAvailable--;

        Bomb b = bombObj.GetComponent<Bomb>();
        if (b != null) b.SetOwner(this);


        if (bombCounterUI != null)
        {
            bombCounterUI.BombPlaced();
        }
    }
    /* revisa todas las bombas que estén bajo el jugador, usado porque un eenmigo puede poner una bomba sobre tí y sin esto estarías atascado */
    void CheckAndIgnoreBombsUnderPlayer()
    {
        Vector2 pos = transform.position;
        Vector2 boxSize = new Vector2(0.9f, 0.9f);
        Collider2D[] bombsUnderPlayer = Physics2D.OverlapBoxAll(pos, boxSize, 0f);
        foreach (var bombCollider in bombsUnderPlayer)
            if (bombCollider != null && bombCollider.CompareTag("Bomb"))
                Physics2D.IgnoreCollision(playerCollider, bombCollider, true);
    }

    public void NotifyBombDestroyed(Collider2D bombCollider)
    {
        if (lastBombCollider == bombCollider && playerCollider != null)
            Physics2D.IgnoreCollision(playerCollider, lastBombCollider, false);

        lastBombCollider = null;
        stillOnLastBomb = false;

        bombsAvailable = Mathf.Min(bombsAvailable + 1, maxBombs);

     
        if (bombCounterUI != null)
        {
            bombCounterUI.BombRemoved();
        }
    } /* OCUPAMOS IBOMBOWNER*/

    public bool EsUltimaBomba(Collider2D col)
    {
        if (!stillOnLastBomb || lastBombCollider == null || col == null) return false;
        if (col == lastBombCollider || col.gameObject == lastBombCollider.gameObject) return true;
        if (col.attachedRigidbody != null && lastBombCollider.attachedRigidbody != null)
            return col.attachedRigidbody.gameObject == lastBombCollider.attachedRigidbody.gameObject;
        return false;
    }

    public void putBomb()
    {

        if (bombsLocked)
        {
            Debug.Log("[PlayerBomb] Bombas bloqueadas durante el escape.");
            return;
        }
        PlaceBomb();
    }
    public Vector3 GetGridPosition(PlayerInputExample playerInput)
    {
        if (gridTilemap != null)
        {
            bool isMoving = playerInput != null && playerInput.IsMoving();
            Vector3Int targetCell = isMoving
                ? prevCell + new Vector3Int(lastCardinalDir.x, lastCardinalDir.y, 0)
                : lastCell;

            return gridTilemap.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, 0f);
        }
        else
        {
            Vector3 p = transform.position;
            return new Vector3(Mathf.Floor(p.x) + 0.5f, Mathf.Floor(p.y) + 0.5f, 0);
        }
    }
}