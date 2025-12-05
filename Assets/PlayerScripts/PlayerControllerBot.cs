using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float moveDelay = 1.5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private BotBomb botBomb;
    public bool frozenOnPlayerDeath = false;
    public void FreezeWhenPlayerDies()
    {
        isMoving = false;
        enabled = false; // desactiva el AI entero
    }

    void Awake()
    {
        targetPosition = transform.position;
        botBomb = GetComponent<BotBomb>();

        // No es error si no tiene BotBomb (quizás es un mob que solo camina), 
        // pero avisamos por si acaso.
        if (botBomb == null)
            Debug.LogWarning("EnemyAI: No se encontró BotBomb en el mismo GameObject.");
    }

    void Start()
    {
        InvokeRepeating(nameof(ChooseRandomMove), 0f, moveDelay);
    }

    void Update()
    {
        if (frozenOnPlayerDeath) return;
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

        // Elegir dirección al azar (0:Izq, 1:Der, 2:Arriba, 3:Abajo)
        int dir = Random.Range(0, 4);
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

        // Usamos un tamaño un poco menor a 1 (0.9) para no detectar paredes vecinas por error
        Vector2 boxSize = new Vector2(0.9f, 0.9f);

        Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)newPos, boxSize, 0f);
        bool blocked = false;

        if (hits != null && hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;

                // 1. BLOQUEO POR PAREDES O CAJAS
                if (hit.CompareTag("blocks") || hit.CompareTag("destructible"))
                {
                    blocked = true;
                    break;
                }

                // 2. BLOQUEO POR BOMBAS
                if (hit.CompareTag("Bomb"))
                {
                    // Si el bot puede poner bombas, revisamos si es la suya recién puesta
                    bool esMiBomba = (botBomb != null && botBomb.EsUltimaBomba(hit));

                    // Si NO es mi bomba (o es una vieja), me bloqueo.
                    if (!esMiBomba)
                    {
                        blocked = true;
                        break;
                    }
                }

                // 3. BLOQUEO POR ICEWALL (NUEVO) ❄️
                if (hit.CompareTag("IceWall"))
                {
                    blocked = true;
                    break;
                }
            }
        }

        // Si no encontré obstáculos, procedo a moverme
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