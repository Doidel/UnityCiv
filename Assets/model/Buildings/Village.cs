using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class Village : ProductionGameBuilding
{
    public GameObject Scaffold;

    public static bool InBuildMode;
    public static Tile BuildModeHoveredTile;
    private Phase1Building BuildModeBuilding;
    private GameObject BuildModeScaffold;
    private BuildItem BuildModeBuildItem;

    public override float ProductionOutput
    {
        get;
        protected set;
    }

    private UnityAction listener;

    // Available items that can be built
    private List<BuildItem> buildItems = new List<BuildItem>();
    public override BuildItem[] Items
    {
        get
        {
            return buildItems.ToArray();
        }

        protected set
        {
            buildItems = new List<BuildItem>(value);
        }
    }

    public override Tile Location
    {
        get;
        set;
    }

    public override BuildOrder Producing
    {
        get;
        protected set;
    }

    public override string Tooltip
    {
        get { return "Build your first settlement"; }
    }

    public override int BuildDurationRounds {
        get { return 0; }
    }

    public override void Produce(BuildItem item)
    {
        var b = item.Produces.GetComponent<Phase1Building>();

        if (b != null)
        {
            // first the player has to select a location where to build
            InBuildMode = true;
            BuildModeBuilding = Instantiate(b);
            BuildModeBuildItem = item;
            // draw the area of this territory tiles
            var territoryTiles = GridManager.instance.board.Values.Where(t => t.InPlayerTerritory);
            foreach (var t in territoryTiles)
            {
                t.SetLooks(1, Tile.TileColorPresets.Area);
                t.DisplayTileResources();
            }
        }
        else
        {
            Producing = new BuildOrder(item, 0);
            TimeManager.instance.NoMoreOrdersNeeded(this);
        }
        Debug.Log("Producing now " + item.Title);
    }

    private Tile lastTile;
    void Update()
    {
        if (BuildModeBuilding != null)
        {
            if (lastTile != BuildModeHoveredTile)
            {
                var previouslyDrawnArea = lastTile == null ? new List<Tile>() : GridManager.instance.GetHexArea(lastTile, BuildModeBuilding.Range);
                var newDrawnArea = GridManager.instance.GetHexArea(BuildModeHoveredTile, BuildModeBuilding.Range).Where(t => GridManager.instance.CalcDistance(Location, t) <= AreaOfControl);
                var leftTiles = previouslyDrawnArea.Where(a => !newDrawnArea.Contains(a)).ToList();
                var newTiles = newDrawnArea.Where(a => !previouslyDrawnArea.Contains(a)).ToList();

                // the center tile needs an update too every time
                if (lastTile != null)
                {
                    leftTiles.Add(lastTile);
                    if (newDrawnArea.Contains(lastTile)) newTiles.Add(lastTile);
                }
                leftTiles.Add(BuildModeHoveredTile);
                newTiles.Add(BuildModeHoveredTile);

                foreach (var t in leftTiles)
                {
                    if (!t.InPlayerTerritory)
                    {
                        t.SetLooks(0, Tile.TileColorPresets.WhiteTransparent);
                        t.HideTileResources();
                    }
                    else
                    {
                        t.DisplayTileResources();
                    }
                }
                foreach (var t in newTiles)
                {
                    t.SetLooks(1, Tile.TileColorPresets.Area);
                    var yieldMods = BuildModeBuilding.TileModifiers(BuildModeHoveredTile, t);
                    // display the tile with the new modifiers, if they exist
                    if (yieldMods != null)
                    {
                        var newYield = t.Yield.ToDictionary(y => y.Key, y => y.Value); // clone
                        foreach (var y in yieldMods.Keys)
                            newYield[y] += yieldMods[y];
                        t.DisplayTileResources(newYield);
                    }
                    else
                    {
                        t.DisplayTileResources();
                    }
                }

                var adjacencyCheck = GridManager.instance.GetHexArea(BuildModeHoveredTile, BuildModeBuilding.Range + 1);
                bool buildable = adjacencyCheck.Any(a => a.InPlayerTerritory);
                BuildModeBuilding.SetColor(buildable ? Phase1Building.ColorPresets.Green : Phase1Building.ColorPresets.Red);

                lastTile = BuildModeHoveredTile;
                BuildModeBuilding.transform.position = GridManager.instance.calcWorldCoordWiggly(new Vector2(lastTile.X + (int)Math.Floor(lastTile.Y / 2d), lastTile.Y));
            }

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var adjacencyCheck = GridManager.instance.GetHexArea(BuildModeHoveredTile, BuildModeBuilding.Range + 1);
                    bool buildable = adjacencyCheck.Any(a => a.InPlayerTerritory);
                    // if at least one tile in or near the area is already player territory
                    if (buildable)
                    {
                        Producing = new BuildOrder(BuildModeBuildItem, 0);
                        BuildModeScaffold = Instantiate(Scaffold);
                        BuildModeScaffold.transform.position = BuildModeBuilding.transform.position;
                        TimeManager.instance.NoMoreOrdersNeeded(this);
                        BuildingPanelUI.instance.SetCurrentlyBuilding();
                    }
                }

                // undo area drawing
                var territoryTiles = GridManager.instance.board.Values.Where(t => t.InPlayerTerritory).ToList();
                territoryTiles.AddRange(GridManager.instance.GetHexArea(BuildModeHoveredTile, BuildModeBuilding.Range));
                foreach (var t in territoryTiles)
                {
                    t.SetLooks(0, Tile.TileColorPresets.WhiteTransparent);
                    t.HideTileResources();
                }

                Destroy(BuildModeBuilding.gameObject);
                BuildModeBuilding = null;
                BuildModeHoveredTile = null;
                InBuildMode = false;
                lastTile = null;
            }
        }
    }

    public override void Select()
    {
        base.Select();
        BuildingPanelUI.instance.SetBuildItems(buildItems.ToArray(), (int)ProductionOutput);
        GridManager.instance.DrawAreaOfControl(Location, AreaOfControl);
    }

    protected override void Start()
    {
        base.Start();
        // set initial items
        buildItems.AddRange(new string[] { "Worker", "Scout" }.Select(s => GameManager.AllBuildItems.First(bi => bi.Title == s)));
			
		// spawn a scout at start
		var scout = GameManager.AllBuildItems.First(bi => bi.Title == "Scout");
		GridManager.instance.Spawn(scout.Produces, new Vector2(Location.X + (int)Math.Floor(Location.Y / 2d), Location.Y));

        // start research
        GameManager.instance.Research.StartResearch();
    }

    protected override void Awake()
    {
        base.Awake();
        listener = new UnityAction(NextRound);
        ProductionOutput = 10;
        Name = "Village";
        Icon = Resources.Load<Sprite>("Icons/btn_campfire");
    }

    void OnEnable()
    {
        EventManager.StartListening("NextRound", listener);
    }

    void OnDisable()
    {
        EventManager.StopListening("NextRound", listener);
    }

    void NextRound()
    {
        if (Producing != null)
        {
            Producing.Produced += ProductionOutput;
            // is the item completed?
            if (Producing.Produced >= Producing.Item.ProductionCosts)
            {
                GameObject produces = (GameObject)Producing.Item.Produces;
                var building = produces.GetComponent<Phase1Building>();
                if (building != null)
                {
                    var go = Instantiate(building);
                    go.transform.position = BuildModeScaffold.transform.position;
                    var coord = GridManager.instance.calcGridCoordStraightAxis(go.transform.position);
                    var tile = GridManager.instance.board[new Point((int)coord.x, (int)coord.y)];
                    tile.Building = go;
                    go.GetComponent<Phase1Building>().Location = tile;
                    Destroy(BuildModeScaffold.gameObject);
                    foreach (var t in GridManager.instance.GetHexArea(tile, building.Range))
                        if (GridManager.instance.CalcDistance(Location, t) <= AreaOfControl)
                            t.InPlayerTerritory = true;
                }
                else
                {
                    var movement = produces.GetComponent<CharacterMovement>();
                    if (movement != null)
                    {
                        var gutest = produces.GetComponent<IGameUnit>();
                        var a = gutest.Actions;
                        GridManager.instance.Spawn(produces, new Vector2(Location.X + (int)Math.Floor(Location.Y / 2d), Location.Y));
                    }

                }
                Producing = null;
                TimeManager.instance.NeedNewOrders(this);
            }

            // update the building display if this building is currently selected
            if (GridManager.instance.selectedBuilding == gameObject)
                BuildingPanelUI.instance.SetCurrentlyBuilding();
        }
    }

    public override bool NeedsOrders()
    {
        return Producing == null;
    }

    public void AddBuildItem(BuildItem item)
    {
        buildItems.Add(item);
    }
}
