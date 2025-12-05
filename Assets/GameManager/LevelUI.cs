using UnityEngine;
using UnityEngine.UI;
using TMPro; // 👈 Importante para usar TextMeshPro

public class LevelUI : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public Slider xpSlider;           // La barra de experiencia
    public TextMeshProUGUI levelText; // El texto que dice "Nivel 1"

    // Actualiza la barra (por ejemplo: 50 / 100 XP)
    public void UpdateXPBar(int currentXP, int maxXP)
    {
        if (xpSlider != null)
        {
            // Convertimos a float para obtener un valor entre 0 y 1
            float fillAmount = (float)currentXP / (float)maxXP;
            xpSlider.value = fillAmount;
        }
    }

    // Actualiza el número de nivel
    public void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Lvl." + level.ToString();
        }
    }
}