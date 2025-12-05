using UnityEngine;
using UnityEngine.Tilemaps;

public class Bomb : MonoBehaviour
{
    public GameObject explosionCenterPrefab;

    public GameObject explosionUpPrefab;
    public GameObject explosionDownPrefab;
    public GameObject explosionLeftPrefab;
    public GameObject explosionRightPrefab;

    public GameObject explosionUpFinalPrefab;
    public GameObject explosionDownFinalPrefab;
    public GameObject explosionLeftFinalPrefab;
    public GameObject explosionRightFinalPrefab;

    public float fuseTime = 2f;
    public int explosionRange = 3;

    private bool exploded = false;

    private Tilemap destructibleTilemap;
    private Tilemap wallTilemap;

    private IBombOwner owner;

    [Header("Audio")]
    public AudioClip explosionClip;        // Sonido al explotar
    public AudioClip placeBombClip;        // 🔊 Sonido al colocar la bomba

    void Start()
    {
        destructibleTilemap = GameObject.Find("Tilemap_destructible").GetComponent<Tilemap>();
        wallTilemap = GameObject.Find("Tilemap_blocks").GetComponent<Tilemap>();

        // 🔊 Reproducir sonido al colocar la bomba
        if (placeBombClip != null)
        {
            AudioSource.PlayClipAtPoint(placeBombClip, transform.position, 1f);
        }

        Invoke(nameof(Explode), fuseTime);
    }

    public void SetOwner(IBombOwner ownerScript)
    {
        owner = ownerScript;
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        // Sonido principal de la bomba
        if (explosionClip != null)
        {
            AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f);
        }

        Vector3 pos = transform.position;
        Instantiate(explosionCenterPrefab, pos, Quaternion.identity);

        PropagateFire(Vector3.up, pos, explosionUpPrefab, explosionUpFinalPrefab);
        PropagateFire(Vector3.down, pos, explosionDownPrefab, explosionDownFinalPrefab);
        PropagateFire(Vector3.left, pos, explosionLeftPrefab, explosionLeftFinalPrefab);
        PropagateFire(Vector3.right, pos, explosionRightPrefab, explosionRightFinalPrefab);

        owner?.NotifyBombDestroyed(GetComponent<Collider2D>());

        Destroy(gameObject);
    }

    [Header("Detector de Obstáculos")]
    public LayerMask obstacleLayer;

    void PropagateFire(Vector3 direction, Vector3 startPos, GameObject explosionPrefab, GameObject explosionFinalPrefab)
    {
        for (int i = 1; i <= explosionRange; i++)
        {
            Vector3 newPos = startPos + direction * i;
            Vector3Int cellPosition = destructibleTilemap.WorldToCell(newPos);
            Vector3 worldPosCentered = destructibleTilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0);

            if (wallTilemap.HasTile(cellPosition))
            {
                break;
            }
            else if (destructibleTilemap.HasTile(cellPosition))
            {
                destructibleTilemap.SetTile(cellPosition, null);

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var stats = player.GetComponent<PlayerStats>();
                    if (stats != null) stats.AddXP(stats.xpPerBlock);
                }

                Instantiate(explosionFinalPrefab, worldPosCentered, Quaternion.identity);
                break;
            }

            Collider2D hitObject = Physics2D.OverlapCircle(worldPosCentered, 0.4f, obstacleLayer);

            if (hitObject != null && hitObject.CompareTag("IceWall"))
            {
                Instantiate(explosionFinalPrefab, worldPosCentered, Quaternion.identity);
                break;
            }

            if (i == explosionRange)
                Instantiate(explosionFinalPrefab, worldPosCentered, Quaternion.identity);
            else
                Instantiate(explosionPrefab, worldPosCentered, Quaternion.identity);
        }
    }
}
