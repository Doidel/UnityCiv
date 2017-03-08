using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Class standing for a human or npc player. e.g. contains their name, and gathers information about them.
/// </summary>
public class Player {

    public readonly string Name;

    public Village Village { get; set; }

    public double GrowthProgress { get; private set; }
    public double ResearchProduction { get; private set; }

    private List<Tile> PopulationPlaces;

    public Player(string name)
    {
        Name = name;
        ResearchProduction = 1;
        GrowthProgress = 0;
        PopulationPlaces = new List<Tile>();
    }

    public Tile[] GetPopulationPlaces()
    {
        return PopulationPlaces.ToArray();
    }

    public void AddPopulation()
    {
        var chosenTile = GetSortedInterestingTiles().First();
        PopulationPlaces.Add(chosenTile);
        chosenTile.IsPopulated = true;
        if (chosenTile.TileResourceIsDisplayed())
            chosenTile.DisplayTileResources();
        EventsDisplay.instance.AddItem(Resources.Load<Sprite>("Icons/population"), "Your population has increased! Your settlement has a size of " + GetPopulationCount() + " now.");
    }

    public void Repopulate()
    {
        foreach (var t in PopulationPlaces)
            t.IsPopulated = false;
        PopulationPlaces.Clear();

        var chosenTiles = GetSortedInterestingTiles();
        PopulationPlaces.AddRange(chosenTiles);
        foreach (var t in chosenTiles)
            if (t.TileResourceIsDisplayed())
                t.DisplayTileResources();
    }

    public int GetPopulationCount()
    {
        return PopulationPlaces.Count;
    }

    public int GetTotalFood()
    {
        return PopulationPlaces.Sum(t => t.GetFinalYield()[Food.i]);
    }

    public int GetTotalProduction()
    {
        return PopulationPlaces.Sum(t => t.GetFinalYield()[Production.i]);
    }

    public double CalculateGrowth()
    {
        var excessFood = GetTotalFood() - GetPopulationCount() * 2;
        return excessFood / 10d;
    }

    public void NextRound()
    {
        GrowthProgress += CalculateGrowth();
        var populationChange = (int)GrowthProgress;
        if (populationChange > 0)
        {
            GrowthProgress -= 1;
            AddPopulation();
            TopPanel.instance.UpdatePopulation(GetPopulationCount());
        }
        TopPanel.instance.UpdatePopulationProgress(GrowthProgress);
        // TODO remove population case
    }

    public IEnumerable<Tile> GetSortedInterestingTiles()
    {
        return GridManager.instance.board.Values.Where(v => v.InPlayerTerritory && !v.IsPopulated).OrderByDescending(t => t.GetFinalYield().Sum(y => y.Value));
    }

    internal bool IsEnemy(Player ownedBy)
    {
        return ownedBy != this;
    }
}
