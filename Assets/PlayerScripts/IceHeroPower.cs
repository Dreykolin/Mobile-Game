using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections;

public class PlayerIceWall : MonoBehaviour, PlayerControls.IPlayerActions
{
    [Header("Paredes de Hielo")]
    public GameObject iceWallPrefab;
    public float wallCooldown = 2f; // tiempo de reutilización en segundos
    private bool canPlaceWall = true;

    [Header("UI")]
    public CooldownWheelUI wallCooldownUI;  // reemplaza el BombCounterUI o agrégalo aparte

    [Header("Grid / Tilemap")]
    [SerializeField] private Tilemap gridTilemap;


    // input
    private PlayerControls controls;

    // colisiones pared-propia
    private Collider2D lastWallCollider;
    private bool stillOnLastWall = false;
    private Collider2D playerCollider;

    // movimiento
    private PlayerInputExample playerInput;
    private Vector2Int lastCardinalDir = Vector2Int.down;
    private Vector3Int lastCell;
    private Vector3Int prevCell;

    [Header("Joystick")]
    public Joystick wallJoystick;

    [Header("Preview")]
    public GameObject wallPreviewPrefab;
    private GameObject wallPreviewInstance;

    private Vector2 lastJoystickInput = Vector2.zero;

    // Debounce para no teletransportar múltiples veces seguidas
    private float lastTeleportTime = -999f;
    private const float teleportDebounce = 0.05f; // 50 ms (ajústalo si quieres)

    private bool wallsLocked = false;



    public void SetWallsLocked(bool locked)
    {
        wallsLocked = locked;

        if (locked && wallPreviewInstance != null)
        {
            Destroy(wallPreviewInstance); // limpia el preview
            wallPreviewInstance = null;
        }
    }
    void Awake()
    {
        playerInput = GetComponent<PlayerInputExample>();
        controls = new PlayerControls();
        controls.player.SetCallbacks(this);
        playerCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (gridTilemap != null)
        {
            lastCell = gridTilemap.WorldToCell(transform.position);
            prevCell = lastCell;
        }

        // Inicializa el wheel como "listo"
        if (wallCooldownUI != null) wallCooldownUI.SetReady();
    }

    void OnEnable() => controls.player.Enable();
    void OnDisable() => controls.player.Disable();

    // ————— INPUT —————
    public void OnFight(InputAction.CallbackContext context)
    {
        if (context.performed)
            PlaceWall();
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

    void Update()
    {
        // --- Track de celda ---
        if (gridTilemap != null)
        {
            var currentCell = gridTilemap.WorldToCell(transform.position);
            if (currentCell != lastCell)
            {
                prevCell = lastCell;
                lastCell = currentCell;
            }
        }

        // --- Manejo de murallas bajo jugador ---
        UpdateWallCollisions();

        if (wallsLocked) return; // 👈 si está bloqueado, no procesa joystick
        // --- Joystick preview y colocación ---
        if (wallJoystick != null)
        {
            Vector2 input = wallJoystick.input;
            Vector2Int dir = Vector2Int.zero;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                dir = new Vector2Int((int)Mathf.Sign(input.x), 0);
            else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                dir = new Vector2Int(0, (int)Mathf.Sign(input.y));

            // Si hay input, actualizamos la dirección y preview
            if (dir != Vector2Int.zero)
            {
                lastCardinalDir = dir;

                Vector3Int baseCell = lastCell;
                Vector3 cellCenter = gridTilemap.GetCellCenterWorld(baseCell);

                Vector2 offset = (Vector2)transform.position - (Vector2)cellCenter;
                Vector2 dirF = new Vector2(dir.x, dir.y).normalized;
                float proj = Vector2.Dot(offset, dirF);

                Vector3Int targetCell = baseCell + new Vector3Int(dir.x, dir.y, 0);
                if (proj > 0f && proj < 0.5f)
                {
                    targetCell += new Vector3Int(dir.x, dir.y, 0);
                }

                Vector3 previewPos = gridTilemap.GetCellCenterWorld(targetCell);

                if (wallPreviewInstance == null)
                    wallPreviewInstance = Instantiate(wallPreviewPrefab);

                wallPreviewInstance.transform.position = previewPos;
            }

            // Al soltar el joystick, colocar
            if (lastJoystickInput != Vector2.zero && input == Vector2.zero && wallPreviewInstance != null)
            {
                if (canPlaceWall)
                {
                    // listo para colocar
                    PlaceWallAtPreview();
                }
                else
                {
                    // en cooldown -> ocultar preview
                    Destroy(wallPreviewInstance);
                    wallPreviewInstance = null;
                }
            }

            lastJoystickInput = input;
        }
    }

    // Debounce (pon esto a nivel de clase si aún no lo tienes)
    // private float lastTeleportTime = -999f;
    // private const float teleportDebounce = 0.05f;

    void UpdateWallCollisions()
    {
        if (Time.time - lastTeleportTime < teleportDebounce) return;

        Vector2 pos = transform.position;
        Vector2 boxSize = new Vector2(0.9f, 0.9f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(pos, boxSize, 0f);
        bool insideIce = false;
        Collider2D anyWall = null;

        foreach (var h in hits)
        {
            if (h != null && h.CompareTag("IceWall"))
            {
                insideIce = true;
                anyWall = h;
                break;
            }
        }
        if (!insideIce) return;

        Debug.Log("⚠️ Pared debajo (post-guard), teletransportando a lastCell...");

        Vector3 backWorldCenter = (gridTilemap != null)
            ? gridTilemap.GetCellCenterWorld(lastCell)
            : new Vector3(Mathf.Floor(transform.position.x) + 0.5f,
                          Mathf.Floor(transform.position.y) + 0.5f, 0f);

        if (playerInput != null) playerInput.ForceSnapTo(backWorldCenter);
        else transform.position = backWorldCenter;

        Physics2D.SyncTransforms();

        if (playerCollider != null && anyWall != null)
            Physics2D.IgnoreCollision(playerCollider, anyWall, false);

        lastWallCollider = null;
        stillOnLastWall = false;

        lastTeleportTime = Time.time;
    }



    bool IsPlayerOnWall(Collider2D wall)
    {
        if (wall == null) return false;

        var hits = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.9f, 0.9f), 0f);
        foreach (var h in hits)
        {
            if (h != null && h.CompareTag("IceWall"))
                return true;
        }
        return false;
    }

    private void ResolvePlayerOverlapWithWall(Collider2D wallCol)
    {
        if (playerCollider == null || wallCol == null) return;

        Physics2D.SyncTransforms();

        Vector2 pos = transform.position;
        Vector2 boxSize = new Vector2(0.9f, 0.9f);

        // ¿Hay una IceWall debajo del player ahora mismo?
        Collider2D[] hits = Physics2D.OverlapBoxAll(pos, boxSize, 0f);
        bool hayParedDebajo = false;
        foreach (var h in hits)
        {
            if (h != null && h.CompareTag("IceWall"))
            {
                hayParedDebajo = true;
                break;
            }
        }
        if (!hayParedDebajo) return;

        Debug.Log("⚠️ Pared debajo tuyo, teletransportando a lastCell...");

        // Tepear a la CASILLA ACTUAL que trackeas (lastCell)
        Vector3 backWorldCenter = (gridTilemap != null)
            ? gridTilemap.GetCellCenterWorld(lastCell)
            : new Vector3(Mathf.Floor(transform.position.x) + 0.5f,
                          Mathf.Floor(transform.position.y) + 0.5f, 0f);

        if (playerInput != null) playerInput.ForceSnapTo(backWorldCenter);
        else transform.position = backWorldCenter;

        Physics2D.SyncTransforms();

        // Asegurar pared sólida
        Physics2D.IgnoreCollision(playerCollider, wallCol, false);
        lastWallCollider = null;
        stillOnLastWall = false;
    }


    private void PlaceWallAtPreview()
    {
        // ⛔️ chequea el lock además del cooldown
        if (wallsLocked || !canPlaceWall || wallPreviewInstance == null) return;

        Vector3 spawnPos = wallPreviewInstance.transform.position;
        Collider2D hit = Physics2D.OverlapBox(spawnPos, new Vector2(0.9f, 0.9f), 0f);
        if (hit != null && (hit.CompareTag("blocks") || hit.CompareTag("destructible") || hit.CompareTag("IceWall")))
        {
            Destroy(wallPreviewInstance);
            wallPreviewInstance = null;
            return;
        }

        GameObject wallObj = Instantiate(iceWallPrefab, spawnPos, Quaternion.identity);
        Collider2D col = wallObj.GetComponent<Collider2D>();
        if (col == null) col = wallObj.GetComponentInChildren<Collider2D>();

        lastWallCollider = col;

        ResolvePlayerOverlapWithWall(col);

        Destroy(wallPreviewInstance);
        wallPreviewInstance = null;

        canPlaceWall = false;
        if (wallCooldownUI != null) wallCooldownUI.Begin(wallCooldown);
        StartCoroutine(WallCooldownRoutine());
    }

    private void PlaceWall()
    {
        // ⛔️ chequea el lock además del cooldown
        if (wallsLocked || !canPlaceWall || wallPreviewInstance == null) return;

        Vector3 spawnPos = wallPreviewInstance.transform.position;
        Collider2D hit = Physics2D.OverlapBox(spawnPos, new Vector2(0.9f, 0.9f), 0f);
        if (hit != null && (hit.CompareTag("blocks") || hit.CompareTag("destructible") || hit.CompareTag("IceWall")))
            return;

        GameObject wallObj = Instantiate(iceWallPrefab, spawnPos, Quaternion.identity);
        Collider2D col = wallObj.GetComponent<Collider2D>();
        if (col == null) col = wallObj.GetComponentInChildren<Collider2D>();

        lastWallCollider = col;

        ResolvePlayerOverlapWithWall(col);

        Destroy(wallPreviewInstance);
        wallPreviewInstance = null;

        canPlaceWall = false;
        if (wallCooldownUI != null) wallCooldownUI.Begin(wallCooldown);
        StartCoroutine(WallCooldownRoutine());
    }

// ✅ Corrutina añadida
private IEnumerator WallCooldownRoutine()
    {
        yield return new WaitForSeconds(wallCooldown);
        canPlaceWall = true;
        if (wallCooldownUI != null) wallCooldownUI.SetReady();
    }

    public void putWall() => PlaceWall();

    public bool EsUltimaPared(Collider2D col)
    {
        if (!stillOnLastWall || lastWallCollider == null || col == null) return false;
        if (col == lastWallCollider || col.gameObject == lastWallCollider.gameObject) return true;
        if (col.attachedRigidbody != null && lastWallCollider.attachedRigidbody != null)
            return col.attachedRigidbody.gameObject == lastWallCollider.attachedRigidbody.gameObject;
        return false;
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 cellCenter = gridTilemap != null
            ? gridTilemap.GetCellCenterWorld(lastCell)
            : new Vector3(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) + 0.5f, 0);
        Gizmos.DrawWireCube(cellCenter, Vector3.one * 0.9f);

        Gizmos.color = Color.cyan;
        Vector3 prevCenter = gridTilemap != null
            ? gridTilemap.GetCellCenterWorld(prevCell)
            : new Vector3(prevCell.x + 0.5f, prevCell.y + 0.5f, 0);
        Gizmos.DrawWireCube(prevCenter, Vector3.one * 0.9f);

        if (wallPreviewInstance != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wallPreviewInstance.transform.position, Vector3.one * 0.9f);
        }
    }
}
