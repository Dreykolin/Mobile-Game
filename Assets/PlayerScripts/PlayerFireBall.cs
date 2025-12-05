using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class PlayerFireBall : MonoBehaviour
{
    [Header("Referencias Generales")]
    public Tilemap gridTilemap;

    [Header("Configuración de Disparo")]
    public GameObject fireballPrefab;
    public float fireballSpeed = 8f;
    public float fireCooldown = 1f;

    [Header("Preview (Prefab del Fantasma)")]
    public GameObject fireballPreviewPrefab; // 👈 Arrastra aquí el PREFAB del fantasma, no un objeto de la escena
    private GameObject previewInstance;      // Referencia interna a la copia creada

    [Header("Referencias de UI")]
    public CooldownWheelUI2 fireballCooldownUI;
    public GameObject fireballCooldownVisual;

    [Header("Control")]
    public Joystick fireJoystick;

    private bool canShoot = true;
    private Vector2 lastJoystickInput = Vector2.zero;
    private Vector2Int lastCardinalDir = Vector2Int.down;

    void Start()
    {
        if (gridTilemap == null)
        {
            gridTilemap = GameObject.Find("Tilemap_destructible")?.GetComponent<Tilemap>();
            if (gridTilemap == null) gridTilemap = FindFirstObjectByType<Tilemap>();
        }
    }

    void Update()
    {
        if (fireJoystick == null) return;
        HandleShootingInput();
    }

    private void HandleShootingInput()
    {
        Vector2 currentInput = fireJoystick.input;
        bool isAiming = currentInput.sqrMagnitude > 0.1f;

        // 1. LÓGICA DE APUNTADO (Joystick presionado)
        if (isAiming)
        {
            // Calcular dirección cardinal
            if (Mathf.Abs(currentInput.x) > Mathf.Abs(currentInput.y))
                lastCardinalDir = new Vector2Int(Mathf.Sign(currentInput.x) > 0 ? 1 : -1, 0);
            else
                lastCardinalDir = new Vector2Int(0, Mathf.Sign(currentInput.y) > 0 ? 1 : -1);

            // Mostrar y actualizar el Preview (Instanciando si no existe)
            if (canShoot)
            {
                UpdatePreview();
            }
        }
        else
        {
            // Ocultar Preview si no estamos apuntando (Destruir)
            ClearPreview();
        }

        // 2. LÓGICA DE DISPARO (Joystick soltado)
        if (lastJoystickInput.sqrMagnitude > 0.1f && !isAiming)
        {
            TryShoot();
        }

        lastJoystickInput = currentInput;
    }

    // Mueve (o crea) el fantasma en la posición exacta
    private void UpdatePreview()
    {
        if (fireballPreviewPrefab == null) return;

        Vector3 spawnPos = GetCalculatedSpawnPosition();

        // LÓGICA TIPO ICEWALL: Si no existe, lo creamos
        if (previewInstance == null)
        {
            previewInstance = Instantiate(fireballPreviewPrefab, spawnPos, Quaternion.identity);
        }

        // Movemos el preview
        previewInstance.transform.position = spawnPos;

        // Rotamos el preview (esto es extra vs Icewall, porque la bola tiene dirección)
        float angle = Mathf.Atan2(lastCardinalDir.y, lastCardinalDir.x) * Mathf.Rad2Deg;
        previewInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Función auxiliar para limpiar (como en IceWall)
    private void ClearPreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
    }

    private void TryShoot()
    {
        if (!canShoot || fireballPrefab == null) return;

        // Destruimos el preview al disparar
        ClearPreview();

        Vector3 spawnPosition = GetCalculatedSpawnPosition();
        Vector3 direction = new Vector3(lastCardinalDir.x, lastCardinalDir.y, 0);

        GameObject fireballInstance = Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);

        FireballProjectile projectile = fireballInstance.GetComponent<FireballProjectile>();
        if (projectile != null)
        {
            projectile.Launch(direction, fireballSpeed);
        }

        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D fireballCollider = fireballInstance.GetComponent<Collider2D>();
        if (playerCollider != null && fireballCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, fireballCollider, true);
        }

        StartCoroutine(FireCooldownRoutine());
    }

    private Vector3 GetCalculatedSpawnPosition()
    {
        Vector3 direction = new Vector3(lastCardinalDir.x, lastCardinalDir.y, 0);

        if (gridTilemap != null)
        {
            Vector3Int playerCell = gridTilemap.WorldToCell(transform.position);
            Vector3 cellCenter = gridTilemap.GetCellCenterWorld(playerCell);
            return cellCenter + (direction * 0.8f);
        }

        return transform.position + (direction * 0.8f);
    }

    private IEnumerator FireCooldownRoutine()
    {
        canShoot = false;
        if (fireballCooldownVisual != null) fireballCooldownVisual.SetActive(true);
        if (fireballCooldownUI != null) fireballCooldownUI.Begin(fireCooldown);

        // Aseguramos que el preview muera si entramos en cooldown
        ClearPreview();

        yield return new WaitForSeconds(fireCooldown);

        canShoot = true;
        if (fireballCooldownVisual != null) fireballCooldownVisual.SetActive(false);
        if (fireballCooldownUI != null) fireballCooldownUI.SetReady();
    }

    void OnEnable()
    {
        if (fireJoystick) fireJoystick.gameObject.SetActive(true);
        if (fireballCooldownVisual) fireballCooldownVisual.SetActive(true);
    }

    void OnDisable()
    {
        if (fireJoystick) fireJoystick.gameObject.SetActive(false);
        if (fireballCooldownVisual) fireballCooldownVisual.SetActive(false);
        ClearPreview(); // Limpieza al desactivar
    }
}