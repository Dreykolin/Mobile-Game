using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownWheelUI2 : MonoBehaviour
{
    [Header("Refs")]
    public Image pieChartImage;
    public TextMeshProUGUI secondsText;

    private float duration;
    private float remaining;
    private bool running;

    public System.Action OnFinished;

    public void Begin(float durationSec)
    {
        duration = Mathf.Max(0.0001f, durationSec);
        remaining = duration;
        running = true;
        UpdateUI();
    }

    public void SetReady()
    {
        running = false;
        remaining = 0f;
        UpdateUI();
    }

    private void Update()
    {
        if (!running) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            running = false;
            UpdateUI();
            OnFinished?.Invoke();
        }
        else
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {

        if (pieChartImage != null)
        {
            float fill = (duration > 0f) ? (remaining / duration) : 0f;
            pieChartImage.fillAmount = Mathf.Clamp01(fill);
        }

        if (secondsText != null)
        {
            secondsText.text = (running && remaining > 0.01f)
                ? Mathf.CeilToInt(remaining).ToString()
                : "";
        }
    }
}
