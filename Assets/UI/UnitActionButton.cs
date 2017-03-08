using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System;

public class UnitActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image Icon;
    public Image ProgressCoverImage;
    public Image ButtonImage;
    public Sprite ButtonNormal;
    public Sprite ButtonActive;

    private UnitAction ua;
    public UnitAction UnitAction {
        get
        {
            return ua;
        }
        set
        {
            ua = value;
            Icon.sprite = ua.Image;
        }
    }

    // makes a circular fill around icon center
    public void SetFillAmount(float amount)
    {
        ProgressCoverImage.fillAmount = amount;
    }

    // makes a focus on icon
    public void SetActionActive(bool active)
    {
        ButtonImage.sprite = active ? ButtonActive : ButtonNormal;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var unit = GridManager.instance.selectedUnit;
        if (unit != null)
            unit.GetComponent<IGameUnit>().HoverAction(UnitAction);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var unit = GridManager.instance.selectedUnit;
        if (unit != null)
            unit.GetComponent<IGameUnit>().LeaveAction(UnitAction);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var unit = GridManager.instance.selectedUnit;
        if (unit != null)
            unit.GetComponent<IGameUnit>().UseAction(UnitAction);
    }
}
