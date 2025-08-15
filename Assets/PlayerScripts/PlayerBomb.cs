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
            [SerializeField] private Tilemap gridTilemap; // ← Asigna aquí tu Tilemap
    
            // input
            private PlayerControls controls;
    
            // colisiones bomba-propia
            private Collider2D lastBombCollider;
            private bool stillOnLastBomb = false;
            private Collider2D playerCollider;
    
            // movimiento (para lógica de celda previa + dir)
            private PlayerInputExample playerInput;
            private Vector2Int lastCardinalDir = Vector2Int.down; // última dirección cardinal
            private Vector3Int lastCell;   // última celda en la que estabas
            private Vector3Int prevCell;   // celda anterior (una “detrás”)
    
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
                // Inicializa celdas
                if (gridTilemap != null)
                {
                    lastCell = gridTilemap.WorldToCell(transform.position);
                    prevCell = lastCell;
                }
            }
    
            void OnEnable() => controls.player.Enable();
            void OnDisable() => controls.player.Disable();
    
            // ————— INPUT —————
            public void OnFight(InputAction.CallbackContext context)
            {
                if (context.performed)
                    PlaceBomb(); // ya NO bloqueamos por IsMoving()
            }
    
            public void OnMove(InputAction.CallbackContext context)
            {
                // Actualizamos la última dirección cardinal para saber "hacia dónde" vas
                Vector2 v = context.ReadValue<Vector2>();
                if (v.sqrMagnitude > 0.0001f)
                {
                    if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
                        lastCardinalDir = new Vector2Int(v.x > 0 ? 1 : -1, 0);
                    else
                        lastCardinalDir = new Vector2Int(0, v.y > 0 ? 1 : -1);
                }
            }
    
            // ————— LOOP —————
            void Update()
            {
                // Track de celda actual y previa
                if (gridTilemap != null)
                {
                    var currentCell = gridTilemap.WorldToCell(transform.position);
                    if (currentCell != lastCell)
                    {
                        prevCell = lastCell;
                        lastCell = currentCell;
                    }
                }
    
                // Liberar colisión con la última bomba propia cuando ya te alejaste
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
    
                // Ignorar bombas bajo el jugador mientras estés encima
                CheckAndIgnoreBombsUnderPlayer();
            }
    
            // ————— CORE —————
            private void PlaceBomb()
            {
                if (bombsAvailable <= 0) return;
    
                Vector3 spawnPos;
    
                if (gridTilemap != null)
                {
                    bool isMoving = playerInput != null && playerInput.IsMoving();
                    Vector3Int bombCell = isMoving
                        ? prevCell + new Vector3Int(lastCardinalDir.x, lastCardinalDir.y, 0)  // celda “frontal”
                        : lastCell;                                                          // celda actual
    
                    // Centro de celda (tu explosión usa +0.5,+0.5, así que mantenemos eso)
                    spawnPos = gridTilemap.CellToWorld(bombCell) + new Vector3(0.5f, 0.5f, 0f);
                }
                else
                {
                    // Fallback sin tilemap: centra en la celda usando floor + centro estándar
                    Vector3 p = transform.position;
                    p = new Vector3(Mathf.Floor(p.x) + 0.5f, Mathf.Floor(p.y) + 0.5f, 0);
                    spawnPos = p;
                }
    
                GameObject bombObj = Instantiate(bombPrefab, spawnPos, Quaternion.identity);
    
                // Ignorar colisión con la bomba recién puesta
                Collider2D col = bombObj.GetComponent<Collider2D>();
                if (col == null) col = bombObj.GetComponentInChildren<Collider2D>();
                lastBombCollider = col;
    
                if (playerCollider != null && lastBombCollider != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, lastBombCollider, true);
                    stillOnLastBomb = true;
                }
    
                bombsAvailable--;
    
                // Set owner
                Bomb b = bombObj.GetComponent<Bomb>();
                if (b != null) b.SetOwner(this);
            }
    
            void CheckAndIgnoreBombsUnderPlayer()
            {
                Vector2 pos = transform.position;
                Vector2 boxSize = new Vector2(0.9f, 0.9f);
                Collider2D[] bombsUnderPlayer = Physics2D.OverlapBoxAll(pos, boxSize, 0f);
                foreach (var bombCollider in bombsUnderPlayer)
                    if (bombCollider != null && bombCollider.CompareTag("Bomb"))
                        Physics2D.IgnoreCollision(playerCollider, bombCollider, true);
            }
    
            // ————— IBombOwner —————
            public void NotifyBombDestroyed(Collider2D bombCollider)
            {
                if (lastBombCollider == bombCollider && playerCollider != null)
                    Physics2D.IgnoreCollision(playerCollider, lastBombCollider, false);
    
                lastBombCollider = null;
                stillOnLastBomb = false;
    
                bombsAvailable = Mathf.Min(bombsAvailable + 1, maxBombs);
            }
    
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
                PlaceBomb();
            }
    
        }
    