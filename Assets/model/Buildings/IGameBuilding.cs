using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// A building or tile improvement in game
/// </summary>
public interface IGameBuilding
{
    [HideInInspector]
    Tile Location { get; set; }
}
