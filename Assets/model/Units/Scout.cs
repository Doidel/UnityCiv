using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Scout : IGameUnit
{
    protected CharacterMovement movement;

    public override void UseAction(UnitAction action)
    {
        base.UseAction(action);
    }

    public override void CancelActions()
    {
        movement.CancelSuggestedMove();
        base.CancelActions();
    }

    public override void Select()
    {
        base.Select();
    }

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<CharacterMovement>();
        movement.MovementPointsMax = 3;
        movement.SightRadius = 2;
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    public override void HoverAction(UnitAction action)
    {
    }

    public override void LeaveAction(UnitAction action)
    {
    }

    public override bool NeedsOrders()
    {
        if (base.NeedsOrders() == false) return false;

        if (movement.RemainingPath != null && (movement.RemainingPath.Count > 1 || movement.MovementPointsRemaining < movement.MovementPointsMax))
        {
            return false;
        }
        return true;
    }

    public override string GetUnitTypeIdentifier()
    {
        return "Scout";
    }
}
