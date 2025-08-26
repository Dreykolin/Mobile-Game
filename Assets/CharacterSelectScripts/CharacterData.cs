using UnityEngine;
using TMPro;

[System.Serializable]
public class ButtonSpec
{
    public string id;         // "bomb", "icewall", "dash", "ult"
    public Sprite icon;
    public string label;
}

[System.Serializable]
public class CharacterUITheme
{
    public Color primary = Color.white;      // color acentos (bordes, barras)
    public Color secondary = Color.cyan;     // color textos/iconos
    public Sprite hudPortrait;               // retrato en HUD (opcional)
    public Sprite abilityIcon;               // icono habilidad (opcional)
    public TMP_FontAsset font;               // fuente opcional
    public GameObject extraHudPrefab;        // extras opcionales
}

[CreateAssetMenu(menuName = "ExploderHeroes/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Info")]
    public string characterName;
    public Sprite portrait;

    [Header("Stats")]
    [Range(1, 10)] public int speed = 5;
    [Range(1, 10)] public int maxBombs = 3;
    [Range(1, 10)] public int explosionRange = 3;

    [Header("Visual")]
    public GameObject skinPrefab;      // opcional: solo visuals
    public GameObject abilityPrefab;   // opcional

    [Header("UI")]
    public CharacterUITheme uiTheme;
    public ButtonSpec[] buttons;       // orden y cantidad de botones visibles
}
