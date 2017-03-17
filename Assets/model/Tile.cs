using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

//Basic skeleton of Tile class which will be used as grid node
public class Tile : GridObject, IHasNeighbours<Tile>
{
    public bool Passable;
    public TerrainType Type;
    public static Sprite[] Sprites;
    public static GameObject TileValuesContainer = new GameObject("TileValues");
    public StrategicResource StrategicResource;
    public bool InPlayerTerritory = false;
    public bool IsPopulated = false;
    public bool IsUncovered { get; private set; }
    public Dictionary<Resource, int> Yield;
    /*{
        get
        {
            return _building == null ? _yield : YieldEmpty;
        }
        set
        {
            _yield = value;
        }
    }*/
    public static Dictionary<Resource, int> YieldEmpty = new Dictionary<Resource, int>()
    {
        { Food.i, 0 },
        { Production.i, 0 }
    };

    private IGameBuilding _building;
    public IGameBuilding Building
    {
        get { return _building; }
        set
        {
            if (_building != null)
            {
                // remove yield modifiers from current building
                var p1buildingOld = _building as Phase1Building;
                if (p1buildingOld != null)
                {
                    var modifiers = p1buildingOld.TileModifiers(this);
                    foreach (var t in GridManager.instance.GetHexArea(this, p1buildingOld.Range))
                    {
                        if (modifiers.ContainsKey(t)) t.RemoveYield(modifiers[t]);
                    }
                }
            }

            _building = value;

            // assign yield modifiers to neighbouring tiles
            var p1building = _building as Phase1Building;
            if (p1building != null)
            {
                var modifiers = p1building.TileModifiers(this);
                foreach (var t in GridManager.instance.GetHexArea(this, p1building.Range))
                {
                    if (modifiers.ContainsKey(t)) t.AddYield(modifiers[t]);
                }
            }
        }
    }

    public enum TerrainType
    {
        DRYEARTH,
        FOREST,
        REDSTONE,
        GRASS,
        RIVER,
        UNASSIGNED = 99
    }

    public Tile(int x, int y)
        : base(x, y)
    {
        Passable = true;
        Type = TerrainType.UNASSIGNED;
        Yield = new Dictionary<Resource, int>()
        {
            { Food.i, 0 },
            { Production.i, 0 }
        };
    }

    /// <summary>
    /// Returns the tile's final yield with all special modifiers applied (isBuilding, percentages)
    /// </summary>
    /// <returns></returns>
    public Dictionary<Resource, int> GetFinalYield()
    {
        if (_building != null && !(_building is Village))
            return YieldEmpty;
        return Yield;
    }

    /// <summary>
    /// Adds yield to the tile's base yield
    /// </summary>
    /// <param name="newYield"></param>
    public void AddYield(Dictionary<Resource, int> newYield)
    {
        if (newYield != null)
            foreach (var y in newYield)
                Yield[y.Key] += y.Value;
    }
    
    /// <summary>
    /// Removes yield to the tile's base yield
    /// </summary>
    /// <param name="newYield"></param>
    public void RemoveYield(Dictionary<Resource, int> newYield)
    {
        if (newYield != null)
            foreach (var y in newYield)
                Yield[y.Key] -= y.Value;
    }

    /// <summary>
    /// Display the tile resources as icons (or a explicit yield, if provided)
    /// </summary>
    public void DisplayTileResources(Dictionary<Resource, int> explicitResources = null)
    {
        if (tileResourceDisplay == null)
        {
            tileResourceDisplay = UnityEngine.Object.Instantiate(GameManager.instance.TileValueDisplayPrefab);
            tileResourceDisplay.transform.position = Representation.transform.position;
            tileResourceDisplay.transform.SetParent(TileValuesContainer.transform);
        }

        tileResourceDisplay.gameObject.SetActive(true);
        var trd = tileResourceDisplay.GetComponent<TileValueDisplay>();
        trd.SetValues(explicitResources == null ? GetFinalYield() : explicitResources, StrategicResource, IsPopulated);
    }
    private GameObject tileResourceDisplay = null;

    /// <summary>
    /// Hide the tile resource icons
    /// </summary>
    public void HideTileResources()
    {
        if (tileResourceDisplay != null)
            tileResourceDisplay.SetActive(false);
    }

    public bool TileResourceIsDisplayed()
    {
        return tileResourceDisplay != null && tileResourceDisplay.activeSelf;
    }

    public void SetTerrainType(TerrainType type)
    {
        // can't set if already set
        if (Type != TerrainType.UNASSIGNED)
            throw new InvalidOperationException("Can't do that... except you want to implement terrain forming");

        switch(type)
        {
            case TerrainType.DRYEARTH:
                Yield[Production.i] += 1;
                break;
            case TerrainType.FOREST:
                Yield[Food.i] += 1;
                break;
            case TerrainType.GRASS:
                Yield[Food.i] += 2;
                break;
            case TerrainType.REDSTONE:
                Yield[Production.i] += 1;
                break;
        }
        Type = type;
    }

    public void SetStrategicResource(StrategicResource resource)
    {
        StrategicResource = resource;
        Yield[Food.i] += resource.BaseYieldFood;
        Yield[Production.i] += resource.BaseYieldProduction;
    }

    public GameObject Representation { get; set; }

    public IEnumerable<Tile> AllNeighbours { get; set; }
    public IEnumerable<Tile> Neighbours
    {
        get { return AllNeighbours.Where(o => o.Passable); }
    }

    //change of coordinates when moving in any direction
    public static List<Point> NeighbourShift
    {
        get
        {
            return new List<Point>
                {
                    new Point(0, 1),
                    new Point(1, 0),
                    new Point(1, -1),
                    new Point(0, -1),
                    new Point(-1, 0),
                    new Point(-1, 1),
                };
        }
    }

    public void FindNeighbours(Dictionary<Point, Tile> Board)
    {
        List<Tile> neighbours = new List<Tile>();

        foreach (Point point in NeighbourShift)
        {
            int neighbourX = X + point.X;
            int neighbourY = Y + point.Y;
            //x coordinate offset specific to straight axis coordinates
            //int xOffset = neighbourY / 2;

            //If every second hexagon row has less hexagons than the first one, just skip the last one when we come to it
            /*if (neighbourY % 2 != 0 && !EqualLineLengths &&
                neighbourX + xOffset == BoardSize.x - 1)
                continue;*/
            //Check to determine if currently processed coordinate is still inside the board limits
            /*if (neighbourX >= 0 - xOffset &&
                neighbourX < (int)BoardSize.x - xOffset &&
                neighbourY >= 0 && neighbourY < (int)BoardSize.y)
                neighbours.Add(Board[new Point(neighbourX, neighbourY)]);*/
            
                Tile nb;
                if (Board.TryGetValue(new Point(neighbourX, neighbourY), out nb))
                    neighbours.Add(nb);
            
        }

        AllNeighbours = neighbours;
    }

    public void SetLooks(int sprite, TileColorPresets color)
    {
        var spr = Representation.GetComponent<SpriteRenderer>();
        spr.sprite = Sprites[sprite];
        spr.color = tileColors[(int)color];
    }

    public enum TileColorPresets
    {
        WhiteTransparent = 0,
        Area = 1,
    }

    private List<Color> tileColors = new List<Color>()
    {
        new Color(1f, 1f, 1f, 0.3f),
        new Color(0.7f, 0.4f, 1f, 0.3f)
    };
    
    private GameObject Fog;
    internal GameObject AreaCover;
    private GameObject PlayerView;

    public void Uncover()
    {
        IsUncovered = true;
        GameObject hexFog = GameObject.Instantiate(GridManager.instance.HexFog);
        hexFog.transform.position = new Vector3(Representation.transform.position.x, hexFog.transform.position.y, Representation.transform.position.z);
        hexFog.transform.parent = Representation.transform;
        Fog = hexFog;
    }

    private int _entityViewCount;
    public int EntityViewCount
    {
        get
        {
            return _entityViewCount;
        }
        set
        {
            _entityViewCount = value;
            if (_entityViewCount > 0 )
            {
                if (PlayerView == null)
                {
                    GameObject playerViewFog = GameObject.Instantiate(GridManager.instance.HexPlayerView);
                    playerViewFog.transform.position = new Vector3(Representation.transform.position.x, playerViewFog.transform.position.y, Representation.transform.position.z);
                    playerViewFog.transform.parent = Representation.transform;
                    PlayerView = playerViewFog;
                }
            }
            else
            {
                GameObject.Destroy(PlayerView);
                PlayerView = null;
            }

            // hide or show enemy players
            var enemies = GridManager.instance.GetEnemiesOnTile(GameManager.instance.LocalPlayer, this);
            foreach (var enemy in enemies)
            {
                var layer = EntityViewCount > 0 ? 0 : 12;
                foreach (Transform trans in enemy.gameObject.GetComponentsInChildren<Transform>(true))
                    trans.gameObject.layer = layer;
            }
        }
    }

    public override string ToString()
    {
        return "Tile[" + X + "," + Y + "]";
    }
}