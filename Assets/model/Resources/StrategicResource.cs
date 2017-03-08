using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Strategic Resources (Deer, Hen, etc.), defined in LevelCreator.cs, attached to Terrain
/// </summary>
[System.Serializable]
public class StrategicResource : Resource
{
    public string Name;
    public GameObject Model;
    public float ChanceOfAppearal;
    public Tile.TerrainType RequiredType;
    public int BaseYieldFood;
    public int BaseYieldProduction;
    public Type ResourceType;

    public enum Type
    {
        Huntable,
        Farmable,
        Fishable
    }
}
