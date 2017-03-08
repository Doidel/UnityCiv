using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public class WildAnimal : IGameUnit
{
    protected CharacterMovement movement;

    public int CurrentDirection = 0;

    public override void UseAction(UnitAction action)
    {
        base.UseAction(action);
    }

    public override void CancelActions()
    {
        movement.CancelSuggestedMove();
        base.CancelActions();
    }

    public override void HoverAction(UnitAction action)
    {
    }

    public override void LeaveAction(UnitAction action) { }

    public override bool NeedsOrders()
    {
        return false;
    }

    public override void Select()
    {
        return;
    }

    protected override void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        movement.MaxHealth = movement.Health = 20;
        movement.Strength = 8;
        movement.MovementPointsMax = 3;

        OwnedBy = GameManager.instance.WildAnimalsPlayer;

        InitUnitCanvas();
        InitAudioSource();
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    void OnEnable()
    {
        EventManager.StartListening("NextRound", nextRoundListener);
    }

    void OnDisable()
    {
        EventManager.StopListening("NextRound", nextRoundListener);
    }

    private void nextRoundListener()
    {
        // implement logic here after each round

        var neighbourTiles = movement.curTile.AllNeighbours.ToList();

        var nextTile = neighbourTiles[CurrentDirection];
		
		// is there an enemy nearby to attack?
		var area = GridManager.instance.GetHexArea(movement.curTile, 2);
        var potentialEnemies = GridManager.instance.allUnits.Where(g => g.OwnedBy == GameManager.instance.LocalPlayer).ToArray();
        var enemy = potentialEnemies.FirstOrDefault(u => area.Contains(u.GetComponent<CharacterMovement>().curTile));
        if (enemy != null)
        {
            // run towards enemy and attack
            movement.AnimWalk = "run";
            movement.MovementSpeed = 1.7f;
            nextTile = enemy.GetComponent<CharacterMovement>().curTile;
        }
        else
        {
            movement.AnimWalk = "walk";
            movement.MovementSpeed = 1f;

            // do we need to reassign currentdir?
            if (!nextTile.Passable || UnityEngine.Random.Range(0f, 1f) <= 0.25f || nextTile.Building is Village)
            {
                CurrentDirection = UnityEngine.Random.Range(0, neighbourTiles.Count - 1);
                nextTile = neighbourTiles[CurrentDirection];
            }
        }

        CancelActions();
        movement.MoveTo(new Vector2(nextTile.X, nextTile.Y));
    }

    // TODO: by name? wolf and bear
    public override string GetUnitTypeIdentifier()
    {
        return "WildAnimal";
    }
}
