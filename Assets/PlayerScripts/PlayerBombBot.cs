using UnityEngine;

public class BotBomb : MonoBehaviour, IBombOwner
{
    public GameObject bombPrefab;
    public int maxBombs = 1; // Máximo de bombas que puede poner el bot
    private int bombsAvailable;

    public float bombCooldown = 2f;
    private float nextBombTime = 0f;

    [Range(0f, 1f)]
    public float bombPlaceProbability = 0.5f; // Probabilidad de poner bomba

    private Collider2D lastBombCollider;
    private bool stillOnLastBomb = false;

    private Collider2D botCollider;

    void Awake()
    {
        botCollider = GetComponent<Collider2D>();
        if (botCollider == null)
            Debug.LogWarning("BotBomb: No se encontró Collider2D en el bot.");

        bombsAvailable = maxBombs;
    }

    void Update()
    {
        // Intento de poner bomba
        if (Time.time >= nextBombTime && bombsAvailable > 0)
        {
            if (Random.value <= bombPlaceProbability)
            {
                PlaceBomb();
            }
            nextBombTime = Time.time + bombCooldown;
        }

        // Manejar colisión con la última bomba puesta
        if (stillOnLastBomb && lastBombCollider != null)
        {
            float distance = Vector2.Distance(transform.position, lastBombCollider.transform.position);
            float threshold = 0.6f;

            if (distance > threshold)
            {
                Physics2D.IgnoreCollision(botCollider, lastBombCollider, false);
                stillOnLastBomb = false;
                lastBombCollider = null;
            }
        }

        // Ignorar bombas debajo del bot
        CheckAndIgnoreBombsUnderBot();
    }

    private void PlaceBomb()
    {
        if (bombsAvailable <= 0) return;

        GameObject bombObj = Instantiate(bombPrefab, transform.position, Quaternion.identity);

        Collider2D col = bombObj.GetComponent<Collider2D>();
        if (col == null) col = bombObj.GetComponentInChildren<Collider2D>();

        lastBombCollider = col;

        if (botCollider != null && lastBombCollider != null)
        {
            Physics2D.IgnoreCollision(botCollider, lastBombCollider, true);
            stillOnLastBomb = true;
        }

        bombsAvailable--;

        // Avisar a la bomba que este bot es su dueño usando IBombOwner
        Bomb bombScript = bombObj.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.SetOwner(this);
        }
    }

    void CheckAndIgnoreBombsUnderBot()
    {
        Vector2 pos = transform.position;
        Vector2 boxSize = new Vector2(0.9f, 0.9f);
        Collider2D[] bombsUnderBot = Physics2D.OverlapBoxAll(pos, boxSize, 0f);

        foreach (var bombCollider in bombsUnderBot)
        {
            if (bombCollider != null && bombCollider.CompareTag("Bomb"))
            {
                Physics2D.IgnoreCollision(botCollider, bombCollider, true);
            }
        }
    }

    // Recuperar bomba cuando explota
    public void RecoverBomb()
    {
        bombsAvailable++;
        if (bombsAvailable > maxBombs) bombsAvailable = maxBombs;
    }

    // Limpiar referencia de la última bomba
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

    // 🔹 Implementación de IBombOwner
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
