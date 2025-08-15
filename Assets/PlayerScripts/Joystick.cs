using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;
    public float handleRange = 50f;

    public Vector2 input = Vector2.zero;

    public float Horizontal => input.x;
    public float Vertical => input.y;

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos);
        pos = Vector2.ClampMagnitude(pos, handleRange);
        handle.anchoredPosition = pos;
        input = pos / handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = Vector2.zero;
        input = Vector2.zero;
    }
}
