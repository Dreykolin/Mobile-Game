using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownWheelUI : MonoBehaviour
{
    [Header("Refs")]
    public Image pieChartImage;           // Image con Fill Method = Radial 360
    public TextMeshProUGUI secondsText;   // opcional (puede ser null)

    // estado interno
    private float duration;       // segundos totales del cooldown
    private float remaining;      // segundos restantes
    private bool running;         // ¿está en cooldown?

    public System.Action OnFinished; // callback opcional al terminar

    /// <summary>Comienza el cooldown visual por 'durationSec' segundos.</summary>
    public void Begin(float durationSec)
    {
        duration = Mathf.Max(0.0001f, durationSec);
        remaining = duration;
        running = true;
        UpdateUI();
    }

    /// <summary>Fuerza a estado 'listo' (sin cooldown visual).</summary>
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
        // Queremos que mientras está en cooldown, el "tapado" radial muestre lo que FALTA.
        // Ej: al empezar: remaining=duration => fill=1; al terminar: remaining=0 => fill=0.
        if (pieChartImage != null)
        {
            float fill = (duration > 0f) ? (remaining / duration) : 0f;
            pieChartImage.fillAmount = Mathf.Clamp01(fill);
        }

        if (secondsText != null)
        {
            secondsText.text = (running && remaining > 0.01f)
                ? Mathf.CeilToInt(remaining).ToString()
                : ""; // vacío cuando está listo
        }
    }
}
