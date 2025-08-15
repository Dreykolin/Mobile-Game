using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BombCounterUI : MonoBehaviour
{
    // Asigna estos objetos en el Inspector
    public Image pieChartImage;
    public TextMeshProUGUI bombCountText;

    // Cantidad máxima y actual de bombas
    private int maxBombs;
    private int currentBombs;

    // Configura la cantidad inicial de bombas
    public void Setup(int initialMaxBombs)
    {
        maxBombs = initialMaxBombs;
        currentBombs = initialMaxBombs;
        UpdateUI();
    }

    // Método para actualizar la UI cuando se planta una bomba
    public void BombPlaced()
    {
        currentBombs--;
        UpdateUI();
    }

    // Método para actualizar la UI cuando una bomba explota o se destruye
    public void BombRemoved()
    {
        currentBombs++;
        if (currentBombs > maxBombs)
        {
            currentBombs = maxBombs; // No puede exceder el máximo
        }
        UpdateUI();
    }

    // Lógica para actualizar los elementos visuales
    private void UpdateUI()
    {
        // Actualiza el texto con la cantidad de bombas
        if (bombCountText != null)
        {
            bombCountText.text = currentBombs.ToString();
        }

        // Actualiza el relleno del pie chart
        if (pieChartImage != null)
        {
            float fillAmount = (float)currentBombs / maxBombs;
            pieChartImage.fillAmount = fillAmount;
        }
    }
}