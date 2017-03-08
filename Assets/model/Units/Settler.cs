using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Settler : IGameUnit
{

    public GameObject VillagePrefab;
    public Sprite icon_expand;
    public AudioClip ExpandAudioClip;

    protected CharacterMovement movement;

    public override void UseAction(UnitAction action)
    {
        base.UseAction(action);
        if (action.Name == "FoundSettlement")
        {
            if (movement.ExpendMovementPoints(2))
            {
                GameObject village = Instantiate(VillagePrefab);
                village.transform.position = transform.position;
                GameManager.instance.LocalPlayer.Village = village.GetComponent<Village>();
                var gb = village.GetComponent<ProductionGameBuilding>();
                gb.Location = movement.curTile;
                gb.Location.AddYield(new Dictionary<Resource, int>()
                {
                    { Food.i, 3 },
                    { Production.i, 1 }
                });
                TimeManager.instance.NoMoreOrdersNeeded(this);
                RemoveCharacter();
                gb.audioSource.PlayOneShot(ExpandAudioClip, 1f);
                LeaveAction(action);
                var affectedArea = GridManager.instance.GetHexArea(gb.Location, 2);
                foreach (var t in affectedArea)
                {
                    t.InPlayerTerritory = true;
                    t.EntityViewCount++;
                }
                gb.Location.Building = gb;
                GameManager.instance.LocalPlayer.AddPopulation();
                TopPanel.instance.UpdatePopulation(1);
                GridManager.instance.UncoverTiles(affectedArea);
            }
        }
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
        movement.MovementPointsMax = 2;
        Actions.Add(new UnitAction()
        {
            Name = "FoundSettlement",
            Description = "Found a settlement",
            Action = VillagePrefab.GetComponent<Village>(),
            ImageAssetPath = "Icons/btn_campfire"
        });
    }

    void Update()
    {
        if (_settlementHexAreaPos != null && _settlementHexAreaPos != movement.curTile)
        {
            GridManager.instance.DrawHexArea(_settlementHexAreaPos, 2, 0, Tile.TileColorPresets.WhiteTransparent);
            HoverAction(Actions[0]);
        }
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
    }
    
    private Tile _settlementHexAreaPos = null;

    public override void HoverAction(UnitAction action)
    {
        GridManager.instance.DrawHexArea(movement.curTile, 2, 1, Tile.TileColorPresets.Area);
        _settlementHexAreaPos = movement.curTile;
    }

    public override void LeaveAction(UnitAction action)
    {
        GridManager.instance.DrawHexArea(movement.curTile, 2, 0, Tile.TileColorPresets.WhiteTransparent);
        _settlementHexAreaPos = null;
    }

    public override bool NeedsOrders()
    {
        if (base.NeedsOrders() == false) return false;

        if (movement.RemainingPath.Count > 1 || movement.MovementPointsRemaining < movement.MovementPointsMax)
        {
            return false;
        }
        return true;
    }

    public override string GetUnitTypeIdentifier()
    {
        return "Settler";
    }
}
