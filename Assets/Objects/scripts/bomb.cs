using UnityEngine;
using UnityEngine.Tilemaps;

public class Bomb : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float fuseTime = 2f;
    public int explosionRange = 3;

    private bool exploded = false;

    // Referencia al Tilemap de bloques destructibles
    private Tilemap destructibleTilemap;
    // Referencia al Tilemap de muros indestructibles
    private Tilemap wallTilemap;

    void Start()
    {
        // Encontrar los Tilemaps por sus nombres y obtener los componentes
        destructibleTilemap = GameObject.Find("Tilemap_destructible").GetComponent<Tilemap>();
        wallTilemap = GameObject.Find("Tilemap_blocks").GetComponent<Tilemap>(); // Asegúrate de que el nombre coincide
        Invoke(nameof(Explode), fuseTime);
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;

        // Instancia la explosión central
        Instantiate(explosionPrefab, pos, Quaternion.identity);

        // Propaga el fuego en las 4 direcciones
        PropagateFire(Vector3.up, pos);
        PropagateFire(Vector3.down, pos);
        PropagateFire(Vector3.left, pos);
        PropagateFire(Vector3.right, pos);

        Destroy(gameObject);
    }

    void PropagateFire(Vector3 direction, Vector3 startPos)
    {
        for (int i = 1; i <= explosionRange; i++)
        {
            Vector3 newPos = startPos + direction * i;
            Vector3Int cellPosition = destructibleTilemap.WorldToCell(newPos);

            // 1. Verificar si la explosión choca con un muro indestructible
            if (wallTilemap.HasTile(cellPosition))
            {
                // Si hay un muro, se detiene la propagación en esta dirección
                break;
            }
            // 2. Si no hay muro, verificar si choca con un bloque destructible
            else if (destructibleTilemap.HasTile(cellPosition))
            {
                // Si hay un bloque destructible, lo destruye, pone la explosión y se detiene
                destructibleTilemap.SetTile(cellPosition, null);
                Instantiate(explosionPrefab, newPos, Quaternion.identity);
                Debug.Log("Tile destruido en: " + cellPosition);
                break; // El fuego no se propaga más allá del bloque destruido
            }
            // 3. Si no hay muro ni bloque, simplemente continúa la propagación
            else
            {
                Instantiate(explosionPrefab, newPos, Quaternion.identity);
            }
        }
    }
}