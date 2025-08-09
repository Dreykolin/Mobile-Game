using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBomb : MonoBehaviour, PlayerControls.IPlayerActions
{
    public GameObject bombPrefab;   // Prefab de la bomba
    public float bombCooldown = 2f; // Tiempo entre bombas
    private float nextBombTime = 0f;

    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.player.SetCallbacks(this);
    }

    void OnEnable()
    {
        controls.player.Enable();
    }

    void OnDisable()
    {
        controls.player.Disable();
    }

    // Método del nuevo sistema de inputs para "poner bomba"
    public void OnFight(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time >= nextBombTime)
        {
            PlaceBomb();
            nextBombTime = Time.time + bombCooldown;
        }
    }

    // Estos no los usamos aquí, pero hay que declararlos por la interfaz
    public void OnMove(InputAction.CallbackContext context) { }

    private void PlaceBomb()
    {
        Instantiate(bombPrefab, transform.position, Quaternion.identity);
    }

    public void putBomb()
    {
        if (Time.time >= nextBombTime)
        {
            PlaceBomb();
            nextBombTime = Time.time + bombCooldown;
        }
    }

}
