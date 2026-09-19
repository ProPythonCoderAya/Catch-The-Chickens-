using UnityEngine;
using UnityEngine.EventSystems;

public class TitleBar : MonoBehaviour, IDragHandler
{
    [SerializeField] private RectTransform window;

    public void OnDrag(PointerEventData eventData)
    {
        window.anchoredPosition += eventData.delta;
    }
}