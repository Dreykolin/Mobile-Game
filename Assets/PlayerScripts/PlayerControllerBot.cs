using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float moveDelay = 1.5f; // Tiempo entre movimientos
    private Vector3 targetPosition;
    private bool isMoving = false;

    private BotBomb botBomb;

    void Awake()
    {
        targetPosition = transform.position;
        botBomb = GetComponent<BotBomb>();
        if (botBomb == null)
            Debug.LogWarning("EnemyAI: No se encontró BotBomb en el mismo GameObject.");
    }

    void Start()
    {
        InvokeRepeating(nameof(ChooseRandomMove), 0f, moveDelay);
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

    void ChooseRandomMove()
    {
        if (isMoving) return;

        int dir = Random.Range(0, 4); // 0=izq, 1=der, 2=arriba, 3=abajo
        Vector3 direction = Vector3.zero;

        switch (dir)
        {
            case 0: direction = Vector3.left; break;
            case 1: direction = Vector3.right; break;
            case 2: direction = Vector3.up; break;
            case 3: direction = Vector3.down; break;
        }

        TryMove(direction);
    }

    void TryMove(Vector3 direction)
    {
        Vector3 newPos = targetPosition + direction;
        Vector2 boxSize = new Vector2(0.9f, 0.9f);

        Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)newPos, boxSize, 0f);
        bool blocked = false;

        if (hits != null && hits.Length > 0)
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
                    bool esMiBomba = (botBomb != null && botBomb.EsUltimaBomba(hit));
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
