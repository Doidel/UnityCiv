using System.Collections;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

//Basic skeleton of Tile class which will be used as grid node
public class Tile : GridObject, IHasNeighbours<Tile>
{
    public bool Passable;

    public Tile(int x, int y)
        : base(x, y)
    {
        Passable = true;
    }

    public IEnumerable AllNeighbours { get; set; }
    public IEnumerable Neighbours
    {
        get { return AllNeighbours.Where(o => o.Passable); }
    }

    IEnumerable<Tile> IHasNeighbours<Tile>.Neighbours
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}