using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BombCounterUI : MonoBehaviour
{
    public Image pieChartImage;
    public TextMeshProUGUI bombCountText;

    private int maxBombs;
    private int currentBombs;

    public void Setup(int initialMaxBombs)
    {
        maxBombs = initialMaxBombs;
        currentBombs = initialMaxBombs;
        UpdateUI();
    }
    public void BombPlaced()
    {
        currentBombs--;
        UpdateUI();
    }

    public void BombRemoved()
    {
        currentBombs++;
        if (currentBombs > maxBombs)
        {
            currentBombs = maxBombs; 
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (bombCountText != null)
        {
            bombCountText.text = currentBombs.ToString();
        }

        if (pieChartImage != null)
        {
            float fillAmount = (float)currentBombs / maxBombs;
            pieChartImage.fillAmount = fillAmount;
        }
    }
}