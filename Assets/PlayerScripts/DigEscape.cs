using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // para ignorar taps sobre UI
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem; // 👈 al inicio
using UnityEngine.UI;
public class DigEscapeTap : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Tilemap gridTilemap;
    [SerializeField] private GameObject targetCursorPrefab; // opcional (visual destino)
    private GameObject targetCursorInstance;

    [Header("Escape Settings")]
    public int maxSteps = 20;             // límite de pasos/expansiones (seguridad)
    public float digStepSpeed = 20f;      // velocidad al “excavar” (unidades/seg)
    public Vector2 overlapBoxSize = new Vector2(0.9f, 0.9f);
    public string[] blockedTags = { "blocks", "destructible", "IceWall", "Bomb" };

    // estado
    private bool targeting = false;
    private Vector3Int startCell;

    private PlayerInputExample playerInput;

    private List<Collider2D> cachedColliders = new List<Collider2D>();
    private int originalLayer = -1;

    // (Opcional) si usas una layer especial para excavar
    [SerializeField] private string subterraneanLayerName = "Subterranean";
    private int subterraneanLayer = -1;
    private SpriteRenderer sr;

    // Túnel atraviesa todo, pero destino debe ser jugable
    [SerializeField] private bool tunnelThroughEverything = true;   // ignora obstáculos en el path
    [SerializeField] private bool requireFreeDestination = true;    // destino debe estar libre

    // === Usos de la habilidad "Dig" ===
    [Header("Dig Uses")]
    [SerializeField] private int digMaxUses = 2;
    [SerializeField] private int digUsesLeft;
    [SerializeField] private Button digButton; // arrástralo en el Inspector
    [SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.35f;


    [Header("UI Lock (avanzado)")]
    [SerializeField] private Selectable[] selectablesToDisable; // Buttons, Sliders, etc.
    [SerializeField] private Behaviour[] scriptsToDisable;      // tus scripts de joystick, etc.
    [SerializeField] private GameObject[] objectsToDisable;     // cosas que quieras SetActive(false)


    [Header("Arrival Telegraph")]
    [SerializeField] private GameObject arrivalMarkerPrefab; // ← arrastra tu cuadrito rojo
    [SerializeField] private float arrivalHoldSeconds = 0.75f; // tiempo inmóvil tras llegar


    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>(); // 👈 busca el sprite
        playerInput = GetComponent<PlayerInputExample>();

        // Cachea todos los colliders del player (por si tienes varios)
        GetComponentsInChildren<Collider2D>(true, cachedColliders);
        digUsesLeft = digMaxUses;

        if (digButton != null)
            digButton.interactable = (digUsesLeft > 0);
        // (Opcional) resuelve el id de la layer Subterranean si existe
        if (!string.IsNullOrEmpty(subterraneanLayerName))
            subterraneanLayer = LayerMask.NameToLayer(subterraneanLayerName);

    }
    // === API: llámalo desde un botón UI para iniciar el targeting por tap ===




    private void EnterIntangible()
    {
        // Apaga colliders
        foreach (var col in cachedColliders)
            if (col) col.enabled = false;

        // Cambia de layer si existe (opcional)
        if (subterraneanLayer != -1)
        {
            originalLayer = gameObject.layer;
            SetLayerRecursively(gameObject, subterraneanLayer);
        }
        if (sr != null)
        {
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0.4f); // 👈 semitransparente
        }
    }

    private void ExitIntangible()
    {
        // Prende colliders
        foreach (var col in cachedColliders)
            if (col) col.enabled = true;

        // Restaura layer
        if (originalLayer != -1)
        {
            SetLayerRecursively(gameObject, originalLayer);
            originalLayer = -1;
        }
        if (sr != null)
        {
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 1f); // 👈 opaco normal
        }
        // Re-sincroniza física
        Physics2D.SyncTransforms();
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursively(t.gameObject, layer);
    }








    [Header("UI Lock")]
    [SerializeField] private CanvasGroup hudGroup; // arrastra el padre del HUD
    [SerializeField] private float lockedAlpha = 0.6f; // opcional

    private void SetUILocked(bool locked)
    {
        if (hudGroup == null) return;
        hudGroup.interactable = !locked;
        hudGroup.blocksRaycasts = !locked;
        // Si quieres atenuar visualmente:
        hudGroup.alpha = locked ? lockedAlpha : 1f;
    }
    public void BeginTapTargeting()
    {
        if (digUsesLeft <= 0)
        {
            Debug.Log("[DigEscapeTap] Sin usos de escape.");
            return;
        }
        if (gridTilemap == null) { Debug.LogWarning("[DigEscapeTap] Falta Tilemap."); return; }

        // 👇 Resetea el sistema de movimiento por si quedaste en medio de un paso
        if (playerInput != null)
            playerInput.ForceSnapTo(transform.position);

        targeting = true;
        startCell = gridTilemap.WorldToCell(transform.position);
        SetUILocked(true);
        // crea/activa cursor si quieres mostrar destino
        if (targetCursorPrefab != null && targetCursorInstance == null)
            targetCursorInstance = Instantiate(targetCursorPrefab);
        if (targetCursorInstance != null)
        {
            targetCursorInstance.SetActive(true);
            targetCursorInstance.transform.position = gridTilemap.GetCellCenterWorld(startCell);
        }

        Debug.Log("[DigEscapeTap] Targeting ON: toca una celda libre para escapar.");
    }

    void Update()
    {
        if (!targeting) return;

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (PointerOverUI()) return;
            TryPickDestination(mouse.position.ReadValue());
        }

        var touches = Touchscreen.current;
        if (touches != null && touches.primaryTouch.press.wasPressedThisFrame)
        {
            if (PointerOverUI()) return;
            TryPickDestination(touches.primaryTouch.position.ReadValue());
        }
    }

    // ¿el tap está sobre UI? (para no robar clicks a botones)
    private bool PointerOverUI(int fingerId = -1)
    {
        if (EventSystem.current == null) return false;
#if UNITY_ANDROID || UNITY_IOS
        if (fingerId >= 0) return EventSystem.current.IsPointerOverGameObject(fingerId);
#endif
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void TryPickDestination(Vector2 screenPos)
    {
        if (gridTilemap == null) return;

        var cam = Camera.main;
        if (cam == null) { Debug.LogWarning("[DigEscapeTap] No hay Camera.main"); return; }

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
        world.z = 0f;

        Vector3Int destCell = gridTilemap.WorldToCell(world);

        // 1) El destino debe ser jugable (libre)
        if (requireFreeDestination && IsBlocked(destCell))
        {
            Debug.Log("[DigEscapeTap] Destino bloqueado (no jugable).");
            EndTargeting();
            return;
        }

        // 2) Origen real al momento del tap
        Vector3Int currentStart = gridTilemap.WorldToCell(transform.position);

        // 3) Construimos el path
        List<Vector3Int> path;
        if (tunnelThroughEverything)
        {
            // Camino Manhattan (primero X, luego Y), IGNORANDO obstáculos
            path = BuildStraightPath(currentStart, destCell, maxSteps);
        }
        else
        {
            // Alternativa: evitar obstáculos (tu BFS original)
            path = FindPathBFS(currentStart, destCell, maxSteps);
        }

        if (path == null || path.Count == 0)
        {
            Debug.Log("[DigEscapeTap] No hay camino dentro del límite.");
            EndTargeting();
            return;
        }

        // Cursor visual (opcional)
        if (targetCursorInstance != null)
            targetCursorInstance.transform.position = gridTilemap.GetCellCenterWorld(destCell);
        digUsesLeft = Mathf.Max(0, digUsesLeft - 1);
        if (digButton != null)
            digButton.interactable = (digUsesLeft > 0);
        // Ejecuta el túnel
        StartCoroutine(CoDigAlong(path));
    }
    private List<Vector3Int> BuildStraightPath(Vector3Int start, Vector3Int goal, int limit)
    {
        var path = new List<Vector3Int>();
        var curr = start;
        path.Add(curr);

        int steps = 0;

        // Avanza en X
        while (curr.x != goal.x && steps <= limit)
        {
            curr += new Vector3Int(curr.x < goal.x ? 1 : -1, 0, 0);
            path.Add(curr);
            steps++;
        }

        // Avanza en Y
        while (curr.y != goal.y && steps <= limit)
        {
            curr += new Vector3Int(0, curr.y < goal.y ? 1 : -1, 0);
            path.Add(curr);
            steps++;
        }

        if (curr != goal) return null; // superó el límite de pasos
        return path;
    }
    private void EndTargeting()
    {
        targeting = false;
        if (targetCursorInstance != null) targetCursorInstance.SetActive(false);
        SetUILocked(false); // <- desbloquear si solo estabas apuntando
        Debug.Log("[DigEscapeTap] Targeting OFF");
    }

    // === Colisión de celdas ===
    private bool IsBlocked(Vector3Int cell)
    {
        Vector3 center = gridTilemap.GetCellCenterWorld(cell);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, overlapBoxSize, 0f);
        if (hits == null) return false;

        foreach (var h in hits)
        {
            if (h == null) continue;
            foreach (var tag in blockedTags)
                if (h.CompareTag(tag))
                    return true;
        }
        return false;
    }

    // === BFS (más corto) con límite suave ===
    private List<Vector3Int> FindPathBFS(Vector3Int start, Vector3Int goal, int limit)
    {
        if (start == goal) return new List<Vector3Int> { start };

        var q = new Queue<Vector3Int>();
        var visited = new HashSet<Vector3Int>();
        var parent = new Dictionary<Vector3Int, Vector3Int>();

        q.Enqueue(start);
        visited.Add(start);

        Vector3Int[] dirs = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

        int expansions = 0;
        int maxExpansions = Mathf.Max(32, limit * 16); // margen seguro

        while (q.Count > 0 && expansions <= maxExpansions)
        {
            var curr = q.Dequeue();
            foreach (var d in dirs)
            {
                var nxt = curr + d;
                if (visited.Contains(nxt)) continue;
                if (IsBlocked(nxt)) continue;

                visited.Add(nxt);
                parent[nxt] = curr;

                if (nxt == goal)
                    return Reconstruct(parent, goal, start);

                q.Enqueue(nxt);
                expansions++;
                if (expansions > maxExpansions) break;
            }
        }
        return null;
    }

    private List<Vector3Int> Reconstruct(Dictionary<Vector3Int, Vector3Int> parent, Vector3Int goal, Vector3Int start)
    {
        var path = new List<Vector3Int>();
        var curr = goal;
        path.Add(curr);
        while (curr != start)
        {
            if (!parent.TryGetValue(curr, out var p)) return null;
            curr = p;
            path.Add(curr);
        }
        path.Reverse();
        return path;
    }

    // === Movimiento por el camino ===
    private IEnumerator CoDigAlong(List<Vector3Int> path)
    {
        // Cierra el modo de selección visual
        EndTargeting();

        // Asegura que el sistema de movimiento no quede en medio de un step
        if (playerInput != null)
            playerInput.ForceSnapTo(transform.position);

        // 🔒 Bloquear acciones mientras excava
        var bomb = GetComponent<PlayerBomb>();
        var walls = GetComponent<PlayerIceWall>();
        if (bomb != null) bomb.SetBombsLocked(true);
        if (walls != null) walls.SetWallsLocked(true);

        // Bloquear input de movimiento durante el túnel
        if (playerInput != null) playerInput.enabled = false;

        // Bloquear botón de escape (anti-spam)
        if (digButton != null) digButton.interactable = false;

        // Modo fantasma (colliders off, alpha, layer opcional)
        EnterIntangible();

        Vector3 lastTarget = transform.position;

        // try/finally para garantizar restauración aunque se interrumpa
        try
        {
            foreach (var cell in path)
            {
                Vector3 target = gridTilemap.GetCellCenterWorld(cell);
                lastTarget = target;

                // Avance suave hacia cada celda del camino
                while (Vector2.Distance(transform.position, target) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        target,
                        digStepSpeed * Time.deltaTime
                    );
                    yield return null;
                }

                // Asegura snap exacto y sincroniza física
                transform.position = target;
                Physics2D.SyncTransforms();

                yield return null; // pequeño respiro por frame
            }
        }
        finally
        {
            // Salir de modo fantasma y restaurar colisiones/visual
            ExitIntangible();

            if (playerInput != null)
                playerInput.ForceSnapTo(lastTarget);

            // Ojo: NO habilitamos playerInput todavía, lo hacemos después de la pausa
        }

        // === Fase de llegada (afuera del try/finally) ===
        // Instancia el marcador en el destino
        GameObject marker = null;
        if (arrivalMarkerPrefab != null)
        {
            marker = Instantiate(arrivalMarkerPrefab, lastTarget, Quaternion.identity);
        }

        // Bloquea la UI mientras se muestra el marcador (opcional)
        SetUILocked(true);

        // Espera el “stun” / hold visible
        yield return new WaitForSeconds(arrivalHoldSeconds);

        // Limpia el marcador
        if (marker != null) Destroy(marker);

        // Ahora sí, reactivar movimiento y desbloquear
        if (playerInput != null)
        {
            playerInput.ForceSnapTo(lastTarget);
            playerInput.enabled = true;
        }

        if (bomb != null) bomb.SetBombsLocked(false);
        if (walls != null) walls.SetWallsLocked(false);

        if (digButton != null)
            digButton.interactable = (digUsesLeft > 0);

        SetUILocked(false);
        UpdateDigButtonUI();

        Debug.Log("[DigEscapeTap] Escape completo.");
    }


    // === Gizmos debug ===
    void OnDrawGizmosSelected()
    {
        if (gridTilemap == null) return;
        Gizmos.color = new Color(1f, 0.2f, 0.7f, 0.6f);
        var c = gridTilemap.WorldToCell(transform.position);
        Gizmos.DrawWireCube(gridTilemap.GetCellCenterWorld(c), new Vector3(overlapBoxSize.x, overlapBoxSize.y, 0f));
    }
    private void UpdateDigButtonUI()
    {
        if (digButton == null) return;

        bool can = (digUsesLeft > 0);
        digButton.interactable = can;
        SetButtonAlpha(can);
    }

    private void SetButtonAlpha(bool enabled)
    {
        var img = digButton.image;
        if (img != null)
        {
            var c = img.color;
            c.a = enabled ? 1f : disabledAlpha;
            img.color = c;
        }
    }

    public void AddDigUses(int delta)
    {
        digUsesLeft = Mathf.Clamp(digUsesLeft + delta, 0, digMaxUses);
        if (digButton != null) digButton.interactable = (digUsesLeft > 0);
    }

    public void ResetDigUses()
    {
        digUsesLeft = digMaxUses;
        if (digButton != null) digButton.interactable = true;
    }
}
