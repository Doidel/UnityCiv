using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class EventDisplayItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image Image;
    public string TooltipText;
    public Action Click;
    [HideInInspector]
    public Vector3 TargetPos;

    public bool DestinationReached()
    {
        return transform.position.y == TargetPos.y;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventsDisplay.instance.EventTooltip.SetContent(TooltipText);
        EventsDisplay.instance.EventTooltip.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventsDisplay.instance.EventTooltip.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            EventsDisplay.instance.EventTooltip.gameObject.SetActive(false);
            EventsDisplay.instance.Items.Remove(this);
            EventsDisplay.instance.RepositionItems();
            Destroy(gameObject);
        }
    }
}
