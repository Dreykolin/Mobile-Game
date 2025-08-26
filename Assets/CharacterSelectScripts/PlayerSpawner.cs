// PlayerSpawner.cs
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab; // tu prefab actual con PlayerInputExample, PlayerIceWall, etc.

    void Awake()
    {
        var go = Instantiate(playerPrefab, transform.position, Quaternion.identity);

        // Aplica los datos del personaje elegido
        var applier = go.GetComponent<PlayerCharacterApplier>();
        if (applier && GameManagerChar.Instance && GameManagerChar.Instance.selectedCharacter)
        {
            applier.ApplyCharacter(GameManagerChar.Instance.selectedCharacter);
        }
    }
}
