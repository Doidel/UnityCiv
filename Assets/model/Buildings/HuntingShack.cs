using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class HuntingShack : Phase1Building {

    protected override void Awake()
    {
        base.Awake();

        Range = 1;
    }

    // Use this for initialization
    void Start () {

    }

    public override Dictionary<Tile, Dictionary<Resource, int>> TileModifiers(Tile position)
    {
        var modifiersDict = new Dictionary<Tile, Dictionary<Resource, int>>();

        var area = GridManager.instance.GetHexArea(position, Range);
        foreach (var tile in area)
        {
            if (tile.StrategicResource != null && tile.StrategicResource.ResourceType == StrategicResource.Type.Huntable)
            {
                modifiersDict.Add(tile, TileModifiers(position, tile));
            }
        }

        return modifiersDict;
    }

    public override Dictionary<Resource, int> TileModifiers(Tile centerTile, Tile affectedTile)
    {
        if (centerTile == null || affectedTile == null)
            return null;

        var baseVal = base.TileModifiers(centerTile, affectedTile);
        if (baseVal != null) return baseVal;

        // range check
        if (GridManager.instance.CalcDistance(centerTile, affectedTile) > Range) //TODO: correct..?
            return null;

        if (affectedTile.StrategicResource != null && affectedTile.StrategicResource.ResourceType == StrategicResource.Type.Huntable)
        {
            return new Dictionary<Resource, int>() {
                { Food.i, 1 }
            };
        }

        return null;
    }
}
