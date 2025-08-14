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

    // Referencia al dueño de la bomba
    private IBombOwner owner;


    void Start()
    {
        destructibleTilemap = GameObject.Find("Tilemap_destructible").GetComponent<Tilemap>();
        wallTilemap = GameObject.Find("Tilemap_blocks").GetComponent<Tilemap>();

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

        Vector3 pos = transform.position;
        Instantiate(explosionCenterPrefab, pos, Quaternion.identity);

        PropagateFire(Vector3.up, pos, explosionUpPrefab, explosionUpFinalPrefab);
        PropagateFire(Vector3.down, pos, explosionDownPrefab, explosionDownFinalPrefab);
        PropagateFire(Vector3.left, pos, explosionLeftPrefab, explosionLeftFinalPrefab);
        PropagateFire(Vector3.right, pos, explosionRightPrefab, explosionRightFinalPrefab);

        // Avisar al dueño (cualquier script que implemente IBombOwner)
        owner?.NotifyBombDestroyed(GetComponent<Collider2D>());

        Destroy(gameObject);
    }

    void PropagateFire(Vector3 direction, Vector3 startPos, GameObject explosionPrefab, GameObject explosionFinalPrefab)
    {
        for (int i = 1; i <= explosionRange; i++)
        {
            Vector3 newPos = startPos + direction * i;
            Vector3Int cellPosition = destructibleTilemap.WorldToCell(newPos);

            if (wallTilemap.HasTile(cellPosition))
            {
                break; // muro indestructible
            }
            else if (destructibleTilemap.HasTile(cellPosition))
            {
                destructibleTilemap.SetTile(cellPosition, null);
                Instantiate(explosionFinalPrefab, destructibleTilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                break;
            }
            else
            {
                if (i == explosionRange)
                    Instantiate(explosionFinalPrefab, destructibleTilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                else
                    Instantiate(explosionPrefab, destructibleTilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            }
        }
    }
}
