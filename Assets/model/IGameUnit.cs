using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public abstract class IGameUnit : AbstractCommandableEntity {
    
    /// <summary>
    /// Actions contain the available actions for each unit.
    /// PrefabActionsAllUnits contains the actions which all newly awoken entities get.
    /// For each unit, e.g. Worker, there can be further actions for newly created entities (PrefabActions).
    /// </summary>
    [HideInInspector]
    public List<UnitAction> Actions;

    public static List<UnitAction> PrefabActionsAllUnits = new List<UnitAction>();

    public void AddAction(UnitAction action)
    {
        if (!Actions.Contains(action))
            Actions.Add(action);
    }

    public void AddAction(string action)
    {
        AddAction(GameManager.UnitActionPrefabs.First(a => a.Name == action));
    }

    [HideInInspector]
    public Nullable<UnitAction> Producing;

    [HideInInspector]
    public short ProducingRoundsLeft;

    // Put actions here which are used by many units
    public virtual void UseAction(UnitAction action)
    {
        if (action.Name == "Sleep")
        {
            if (!Producing.HasValue || Producing.Value.Name != action.Name)
            {
                CancelActions();
                Producing = action;
                ProducingRoundsLeft = short.MaxValue;
            }
            else
            {
                CancelActions();
            }
            UnitPanelUI.instance.SetUnit(this);
        }
    }

    public virtual void CancelActions()
    {
        Producing = null;
        ProducingRoundsLeft = 0;
        UnitPanelUI.instance.SetUnit(this);
    }

    public abstract void HoverAction(UnitAction action);

    public abstract void LeaveAction(UnitAction action);

    public override void Select()
    {
        GridManager.instance.selectedUnit = gameObject;
        BuildingPanelUI.instance.SetBuildItems(null, 0);
        UnitPanelUI.instance.SetUnit(this);
        audioSource.PlayOneShot(GameManager.instance.Select1, 0.2f);
        var movement = gameObject.GetComponent<CharacterMovement>();
        if (movement != null)
            movement.DrawSuggestedPath();
    }

    public abstract string GetUnitTypeIdentifier();

    protected override void Awake()
    {
        base.Awake();
        Actions = PrefabActionsAllUnits.ToList();
        AddAction("Sleep");
    }

    protected virtual void Start()
    {
    }

    /// <summary>
    /// Remove a unit/character from the game with this method instead of directly calling "Destroy"
    /// </summary>
    public void RemoveCharacter(float after = 0f)
    {
        if (GridManager.instance.selectedUnit == gameObject)
        {
            GridManager.instance.selectedUnit = null;
            UnitPanelUI.instance.SetUnit(null);
        }
        GridManager.instance.allUnits.Remove(this);
        Destroy(gameObject, after);

    }

    public override bool NeedsOrders()
    {
        if (Producing.HasValue && Producing.Value.Name == "Sleep")
            return false;
        return true;
    }
}
