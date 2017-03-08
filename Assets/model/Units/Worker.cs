using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Worker : IGameUnit {

    public AudioClip WorkerStartWork;
    protected CharacterMovement movement;

    public static List<UnitAction> PrefabActions = new List<UnitAction>();

    private GameObject ProductionScaffold;
    
    public override void UseAction(UnitAction action)
    {
        base.UseAction(action);

        var actionGO = action.Action as GameObject;
        if (actionGO != null)
        {
            var p1TileImprovement = actionGO.GetComponent<Phase1TileImprovement>();
            if (p1TileImprovement != null)
            {
                // abort current activity, or current construction if it isn't the same
                if (Producing.HasValue && Producing.Value.Action != action.Action)
                    CancelActions();
                Producing = action;
                ProducingRoundsLeft = p1TileImprovement.BuildDurationRounds;
                ProductionScaffold = Instantiate(GameManager.instance.ScaffoldPrefab);
                ProductionScaffold.transform.position = new Vector3(movement.curTilePos.x, movement.curTilePos.y, movement.curTilePos.z);
                audioSource.PlayOneShot(WorkerStartWork);
            }
        }
    }

    public override void CancelActions()
    {
        Destroy(ProductionScaffold);
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
        Actions.AddRange(PrefabActions);
        movement = GetComponent<CharacterMovement>();
        movement.MovementPointsMax = 2;
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

        if (movement.RemainingPath.Count > 1 || Producing != null || movement.MovementPointsRemaining < movement.MovementPointsMax)
        {
            return false;
        }
        return true;
    }

    void OnEnable()
    {
        EventManager.StartListening("NextRoundRequest", nextRoundRequestListener);
        EventManager.StartListening("NextRound", nextRoundListener);
    }

    void OnDisable()
    {
        EventManager.StopListening("NextRoundRequest", nextRoundRequestListener);
        EventManager.StopListening("NextRound", nextRoundListener);
    }

    private void nextRoundRequestListener()
    {
        if (Producing != null && movement.ExpendMovementPoints(2))
        {
            // perform some building action
            // TODO: Animation
            ProducingRoundsLeft--;
            if (ProducingRoundsLeft <= 0)
            {
                Destroy(ProductionScaffold);
                var newImprovement = Instantiate((Phase1TileImprovement)Producing.Value.Action);
                newImprovement.transform.position = new Vector3(movement.curTilePos.x, movement.curTilePos.y, movement.curTilePos.z);
                newImprovement.Location = movement.curTile;
                movement.curTile.Building = newImprovement;
                GameManager.instance.AddTileImprovement(newImprovement);
                Producing = null;
                //if (GridManager.instance.selectedUnit == gameObject)
                    //TODO: Update icon progress display of production
            }
        }
    }

    private void nextRoundListener()
    {
        if (GridManager.instance.selectedUnit == gameObject)
            UnitPanelUI.instance.SetUnit(this);
    }

    public override string GetUnitTypeIdentifier()
    {
        return "Worker";
    }
}
