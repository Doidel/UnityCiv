using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ResearchEffects
{
    public static void UnlockBuilding(BuildItem building)
    {
        GameManager.instance.LocalPlayer.Village.AddBuildItem(building);
    }

    public static void UnlockUnit(BuildItem unit)
    {
        GameManager.instance.LocalPlayer.Village.AddBuildItem(unit);
    }

    public static void UnlockUnitAction(string action)
    {
        var unitAction = GameManager.UnitActionPrefabs.First(ua => ua.Name == action);

        var unitActionGO = (GameObject)unitAction.Action;
        IGameUnit[] relevantUnits = null;
        if (unitActionGO.GetComponent<Phase1TileImprovement>() != null)
        {
            relevantUnits = GridManager.instance.allUnits.Where(u => u.OwnedBy == GameManager.instance.LocalPlayer && u is Worker).ToArray();
            Worker.PrefabActions.Add(unitAction);
        }
        
        foreach (var unit in relevantUnits)
        {
            unit.AddAction(unitAction);
            if (GridManager.instance.selectedUnit == unit)
                unit.Select();
        }
    }

    public static void ChangeYield(string strategicResource, Dictionary<Resource, int> yieldChange)
    {
        var sr = LevelCreator.instance.StrategicResources.First(s => s.Name == strategicResource);
        sr.BaseYieldFood += yieldChange[Food.i];
        sr.BaseYieldProduction += yieldChange[Production.i];
        // change the yield of already existing tiles
        var tiles = GridManager.instance.board.Values.Where(v => v.StrategicResource != null && v.StrategicResource.Name == strategicResource).ToArray();
        foreach (var tile in tiles)
            tile.AddYield(yieldChange);
    }
}
