// PlayerCharacterApplier.cs
using UnityEngine;

public class PlayerCharacterApplier : MonoBehaviour
{
    public void ApplyCharacter(CharacterData c)
    {
        if (c == null) return;

        // Velocidad → a tu controlador de movimiento
        var input = GetComponent<PlayerInputExample>();
        if (input)
        {
            // mapea 1..10 a una velocidad razonable (ajusta a gusto)
            input.moveSpeed = Mathf.Lerp(3f, 7.5f, (c.speed - 1) / 9f);
        }

        // Bombas máximas → a tu sistema de bombas
        var bomb = GetComponent<PlayerBomb>();
        if (bomb)
        {
            // Implementa este setter en tu PlayerBomb si no existe
            bomb.SetMaxBombs(c.maxBombs);
        }

        // (Opcional) Instanciar skin visual si usas uno por personaje:
        // if (c.skinPrefab) Instantiate(c.skinPrefab, transform);
    }
}
