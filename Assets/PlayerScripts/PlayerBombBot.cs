using UnityEngine;
using UnityEngine.Tilemaps;

public class BotBomb : MonoBehaviour, IBombOwner
{
    public GameObject bombPrefab;
    public int maxBombs = 1;
    private int bombsAvailable;

    public float bombCooldown = 2f;
    private float nextBombTime = 0f;

    [Range(0f, 1f)]
    public float bombPlaceProbability = 0.5f;

    private Collider2D lastBombCollider;
    private bool stillOnLastBomb = false;
    private Collider2D botCollider;
    private Rigidbody2D rb;

    // Grid
    [SerializeField] private Tilemap gridTilemap;
    private Vector2Int lastCardinalDir = Vector2Int.down;
    private Vector3Int lastCell;
    private Vector3Int prevCell;

    void Awake()
    {
        botCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        bombsAvailable = maxBombs;
    }

    void Start()
    {
        if (gridTilemap != null)
        {
            lastCell = gridTilemap.WorldToCell(transform.position);
            prevCell = lastCell;
        }
    }

    private bool IsMoving()
    {
        return rb.linearVelocity.sqrMagnitude > 0.01f;
    }

    void Update()
    {
        // Track celda y dirección
        if (gridTilemap != null)
        {
            var currentCell = gridTilemap.WorldToCell(transform.position);
            if (currentCell != lastCell)
            {
                prevCell = lastCell;
                lastCell = currentCell;

                Vector3 delta = currentCell - lastCell;
                if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                    lastCardinalDir = new Vector2Int(delta.x > 0 ? 1 : -1, 0);
                else
                    lastCardinalDir = new Vector2Int(0, delta.y > 0 ? 1 : -1);
            }
        }

        // Intento de colocar bomba
        if (Time.time >= nextBombTime && bombsAvailable > 0)
        {
            if (!IsMoving() && Random.value <= bombPlaceProbability)
                PlaceBomb();

            nextBombTime = Time.time + bombCooldown;
        }

        // Manejar colisión con la última bomba puesta
        if (stillOnLastBomb && lastBombCollider != null)
        {
            float distance = Vector2.Distance(transform.position, lastBombCollider.transform.position);
            if (distance > 0.6f)
            {
                Physics2D.IgnoreCollision(botCollider, lastBombCollider, false);
                stillOnLastBomb = false;
                lastBombCollider = null;
            }
        }

        CheckAndIgnoreBombsUnderBot();
    }

    private void PlaceBomb()
    {
        if (bombsAvailable <= 0) return;

        Vector3 spawnPos;
        if (gridTilemap != null)
        {
            Vector3Int bombCell = IsMoving() ? prevCell + new Vector3Int(lastCardinalDir.x, lastCardinalDir.y, 0) : lastCell;
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

        if (botCollider != null && lastBombCollider != null)
        {
            Physics2D.IgnoreCollision(botCollider, lastBombCollider, true);
            stillOnLastBomb = true;
        }

        bombsAvailable--;

        Bomb b = bombObj.GetComponent<Bomb>();
        if (b != null) b.SetOwner(this);
    }

    void CheckAndIgnoreBombsUnderBot()
    {
        Collider2D[] bombsUnderBot = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.9f, 0.9f), 0f);
        foreach (var c in bombsUnderBot)
            if (c != null && c.CompareTag("Bomb"))
                Physics2D.IgnoreCollision(botCollider, c, true);
    }

    public void RecoverBomb()
    {
        bombsAvailable = Mathf.Min(bombsAvailable + 1, maxBombs);
    }

    public void ClearLastBombReference(Collider2D bombCollider)
    {
        if (lastBombCollider == bombCollider)
        {
            if (botCollider != null && lastBombCollider != null)
                Physics2D.IgnoreCollision(botCollider, lastBombCollider, false);

            lastBombCollider = null;
            stillOnLastBomb = false;
        }
    }

    public void NotifyBombDestroyed(Collider2D bombCollider)
    {
        ClearLastBombReference(bombCollider);
        RecoverBomb();
    }

    public bool EsUltimaBomba(Collider2D col)
    {
        if (!stillOnLastBomb || lastBombCollider == null || col == null) return false;
        if (col == lastBombCollider || col.gameObject == lastBombCollider.gameObject) return true;
        if (col.attachedRigidbody != null && lastBombCollider.attachedRigidbody != null)
            return col.attachedRigidbody.gameObject == lastBombCollider.attachedRigidbody.gameObject;
        return false;
    }
}

