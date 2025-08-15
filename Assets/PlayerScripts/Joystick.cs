using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;
    public float handleRange = 50f;

    // Se mantiene p�blico para que PlayerInputExample pueda leerlo
    public Vector2 input = Vector2.zero;

    public float Horizontal => input.x;
    public float Vertical => input.y;

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos);

        // Clampa la posici�n del handle para que no se salga del fondo
        pos = Vector2.ClampMagnitude(pos, handleRange);
        handle.anchoredPosition = pos;

        // --- L�gica de input modificada ---
        // Aqu� es donde el magic sucede. Primero calculamos el input como antes.
        Vector2 rawInput = pos / handleRange;

        // Ahora, en lugar de usar el rawInput, lo normalizamos a 4 direcciones
        // para que sea m�s f�cil cambiar de direcci�n.
        if (Mathf.Abs(rawInput.x) > Mathf.Abs(rawInput.y))
        {
            // Si el movimiento horizontal es dominante, el input vertical es 0
            input = new Vector2(Mathf.Sign(rawInput.x), 0);
        }
        else if (Mathf.Abs(rawInput.y) > Mathf.Abs(rawInput.x))
        {
            // Si el movimiento vertical es dominante, el input horizontal es 0
            input = new Vector2(0, Mathf.Sign(rawInput.y));
        }
        else
        {
            // Si no hay un movimiento dominante (est� en el centro), el input es 0
            input = Vector2.zero;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = Vector2.zero;
        input = Vector2.zero;
    }
}