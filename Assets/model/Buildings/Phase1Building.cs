using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public abstract class Phase1Building : AbstractCommandableEntity, IGameBuilding
{

    [HideInInspector]
    public Tile Location { get; set; }

    [HideInInspector]
    public int Range;
    
    /// <summary>
    /// How are affected tiles yields changed by this building?
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public abstract Dictionary<Tile, Dictionary<Resource, int>> TileModifiers(Tile centerTile);
    
    /// <summary>
    /// How is a specific tile's yield affected by this building?
    /// </summary>
    /// <param name="centerTile"></param>
    /// <param name="affectedTile"></param>
    /// <returns></returns>
    public virtual Dictionary<Resource, int> TileModifiers(Tile centerTile, Tile affectedTile)
    {
        // if it's the building's tile, negate its value
        if (centerTile == affectedTile)
            return centerTile.Yield.ToDictionary(y => y.Key, y => -y.Value);
        return null;
    }

    public override bool NeedsOrders()
    {
        return false;
    }

    public override void Select()
    { }

    public enum ColorPresets
    {
        Green,
        Red,
        Transparent
    }

   
    public void SetColor(ColorPresets colorPreset)
    {
        Renderer renderer = GetComponentInChildren<MeshRenderer>();

        Color color;

        switch (colorPreset)
        {
            case ColorPresets.Green:
                color = new Color(0, 0, 0, 1);
                break;
            case ColorPresets.Red:
                color = new Color(0.3f, 0, 0, 1);
                break;
            default:
                color = new Color(0, 0, 0, 1);
                break;
        }

        for (int i = 0; i < renderer.materials.Length; i++)
        {
            renderer.materials[i].EnableKeyword("_EMISSION");
            renderer.materials[i].SetColor("_EmissionColor", color);
        }
    }
}
