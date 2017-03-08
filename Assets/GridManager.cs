using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// Hex coming from:
/// http://www.redblobgames.com/grids/hexagons/
/// https://tbswithunity3d.wordpress.com/2012/02/23/hexagonal-grid-path-finding-using-a-algorithm/
/// http://keekerdc.com/2011/03/hexagon-grids-coordinate-systems-and-distance-calculations/
/// </summary>
public class GridManager : MonoBehaviour
{
    public GameObject Hex;
    public GameObject HexFog;
    public GameObject HexPlayerView;

    public GameObject MovementLineObject;

    public Sprite[] tileSprites;

    //selectedTile stores the tile mouse cursor is hovering on
    public Tile selectedTile = null;
    //tile which is the start of the path

    //board is used to store tile locations
    public Dictionary<Point, Tile> board = new Dictionary<Point, Tile>();
    //Line should be initialised to some 3d object that can fit nicely in the center of a hex tile and will be used to indicate the path. For example, it can be just a simple small sphere with some material attached to it. Initialise the variable using inspector pane.
    public GameObject Line;
    //List to hold "Lines" indicating the path
    List<GameObject> path;

    [HideInInspector]
    public GameObject selectedUnit = null;
    [HideInInspector]
    public GameObject selectedBuilding = null;
    [HideInInspector]
    public List<IGameUnit> allUnits = new List<IGameUnit>();
    [HideInInspector]
    public List<GameObject> allBuildings = new List<GameObject>();
    [HideInInspector]
    public List<Phase1TileImprovement> allTileImprovements = new List<Phase1TileImprovement>();

    public List<RectTransform> uiElements = new List<RectTransform>();
    public List<Vector3[]> worldCorners = new List<Vector3[]>();

    public static GridManager instance = null;

    public float hexWidth
    {
        get;
        private set;
    }
    public float hexHeight
    {
        get;
        private set;
    }
    public float groundWidth { get; private set; }
    [HideInInspector]
    public Vector2 gridSize;

    void Awake()
    {
        instance = this;
        Tile.Sprites = tileSprites;
    }

    void setSizes()
    {
        //hexWidth = Hex.GetComponent<Renderer>().bounds.size.x;
        hexHeight = Hex.GetComponent<Renderer>().bounds.size.z;
        // we are using pointy topped hexagons. "The width of a hexagon is width = sqrt(3)/2 * height."
        hexWidth = (float)(Math.Sqrt(3) / 2 * hexHeight);

        /*var terrainComponent = Ground.GetComponent<Terrain>();
        if (terrainComponent != null)
        {
            groundWidth = terrainComponent.terrainData.size.x;
            groundHeight = terrainComponent.terrainData.size.z;
        }
        else
        {
            groundWidth = Ground.GetComponent<Renderer>().bounds.size.x;
            groundHeight = Ground.GetComponent<Renderer>().bounds.size.z;
        }*/


        var t = LevelCreator.instance.TerrainPrefab.GetComponent<Terrain>();
        t.terrainData.size = new Vector3(hexHeight * 1.5f * 10f, t.terrainData.size.y, hexHeight * 1.5f * 12f);
        groundWidth = t.terrainData.size.x;
        /*if (t.terrainData.size.z != groundWidth)
            throw new InvalidProgramException("Terrain is not square");*/
        //groundWidth = 40;
        //groundHeight = 40;

        //gridSize = calcGridSize(groundHeight, groundWidth);
    }

    //The method used to calculate the number hexagons in a row and number of rows
    //Vector2.x is gridWidthInHexes and Vector2.y is gridHeightInHexes
    Vector2 calcGridSize(float height, float width)
    {
        //According to the math textbook hexagon's side length is half of the height
        float sideLength = hexHeight / 2;
        //the number of whole hex sides that fit inside inside ground height
        int nrOfSides = (int)(height / sideLength);
        //I will not try to explain the following calculation because I made some assumptions, which might not be correct in all cases, to come up with the formula. So you'll have to trust me or figure it out yourselves.
        int gridHeightInHexes = (int)(nrOfSides * 2 / 3);
        //When the number of hexes is even the tip of the last hex in the offset column might stick up.
        //The number of hexes in that case is reduced.
        if (gridHeightInHexes % 2 == 0
            && (nrOfSides + 0.5f) * sideLength > height)
            gridHeightInHexes--;
        //gridWidth in hexes is calculated by simply dividing ground width by hex width
        return new Vector2((int)(width / hexWidth), gridHeightInHexes);
    }

    Vector2 calcGridSize2(float height, float width)
    {
        //According to the math textbook hexagon's side length is half of the height
        float sideLength = hexHeight / 2;
        //the number of whole hex sides that fit inside inside ground height
        int nrOfSides = (int)(height / (sideLength * 1.5));
        return new Vector2((int)(width / hexWidth), nrOfSides);
    }

    //Method to calculate the position of the first hexagon tile
    //The center of the hex grid is (0,0,0)
    public Vector3 calcInitPos()
    {
        Vector3 initPos;
        initPos = new Vector3(-groundWidth / 2 + hexWidth / 2, 0,
            groundWidth / 2 - hexWidth / 2);

        return initPos;
    }

    /// <summary>
    /// calcs world coord from wiggly axis points. If you have straight axis coordinate system, remember to add x+=y/2
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 calcWorldCoordWiggly(Vector2 gridPos)
    {
        Vector3 initPos = calcInitPos();
        float offset = 0;
        if (gridPos.y % 2 != 0)
            offset = hexWidth / 2;

        float x = initPos.x + offset + gridPos.x * hexWidth;
        float z = initPos.z - gridPos.y * hexHeight * 0.75f;
        //If your ground is not a plane but a cube you might set the y coordinate to sth like groundDepth/2 + hexDepth/2
        return new Vector3(x, 0.01f, z);
    }

    /// <summary>
    /// Calcs world coord from tiles, assuming straight axis coordinate systems. Usually use this one.
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 calcWorldCoord(Vector2 gridPos)
    {
        return calcWorldCoordWiggly(new Vector2(gridPos.x + (int)Math.Floor(gridPos.y / 2), gridPos.y));
    }

    public Vector2 calcGridCoordStraightAxis(Vector3 worldPos)
    {
        Vector3 initPos = calcInitPos();

        float y = -(float)(worldPos.z - initPos.z) / (hexHeight * 0.75f);
        float x = (worldPos.x - initPos.x) / hexWidth - y/2;
        return hex_round(new Vector2(x, y));

        /*var w = new Vector3(worldPos.x - initPos.x, 0f, worldPos.z - initPos.z);
        var q = (w.x * (float)Math.Sqrt(3) / 3f + w.z / 3f) / (hexHeight / 2f);
        var r = -w.z * 2 / 3 / (hexHeight / 2);
        var res = hex_round(new Vector2(q, r));
        //Debug.Log("unrounded " + q + ", " + r + " - rounded " + res.x + ", " + res.y);
        return res;*/
    }
    public Vector2 calcGridCoordWigglytAxis(Vector3 worldPos)
    {
        Vector3 initPos = calcInitPos();

        float y = -(float)(worldPos.z - initPos.z) / (hexHeight * 0.75f);
        float x = (worldPos.x - initPos.x) / hexWidth;
        return hex_round(new Vector2(x, y));
    }

    public Vector2 hex_round(Vector2 h)
    {
        return cube_to_hex(cube_round(hex_to_cube(h)));
    }

    // See http://www.redblobgames.com/grids/hexagons/#rounding
    public Vector3 cube_round(Vector3 cubeInput) {
        var rx = Math.Round(cubeInput.x);
        var ry = Math.Round(cubeInput.y);
        var rz = Math.Round(cubeInput.z);

        var x_diff = Math.Abs(rx - cubeInput.x);
        var y_diff = Math.Abs(ry - cubeInput.y);
        var z_diff = Math.Abs(rz - cubeInput.z);

        //reset largest rounding change
        if (x_diff > y_diff && x_diff > z_diff) {
            rx = -ry - rz;
        } else if (y_diff > z_diff) {
            ry = -rx - rz;
        } else {
            rz = -rx - ry;
        }

        return new Vector3((float)rx, (float)ry, (float)rz);
    }

    // ranges from 0 to 1
    public float cube_distance(Vector3 a, Vector3 b)
    {
        return (Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) + Math.Abs(a.z - b.z)) / 2;
    }

    public Vector2 cube_to_hex(Vector3 h)
    {
        return new Vector2(h.x, h.z);
    }

    public Vector3 hex_to_cube(Vector2 h)
    {
        return new Vector3(h.x, -h.x - h.y, h.y);
    }

    GameObject hexGridContainer;

    //TODO: Use terrain overlapping grid by 1 for each edge, use already existing board tiles
    public Dictionary<Point, Tile> CreateOrGetGrid(Vector2 terrainTilePos)
    {
        var affectedBoardTiles = new Dictionary<Point, Tile>();        

        //overlap
        float gridOverlapWorldUnits = 2f;

        // for terrain generation we have to invert y, because in Unity y (applied as world "z") is opposite to tile y direction
        var corners = new Vector2[2] {
            calcGridCoordWigglytAxis(new Vector3(terrainTilePos.x * groundWidth -  groundWidth / 2f - gridOverlapWorldUnits, 0f, terrainTilePos.y * groundWidth +  groundWidth / 2f + gridOverlapWorldUnits)),
            calcGridCoordWigglytAxis(new Vector3(terrainTilePos.x * groundWidth +  groundWidth / 2f + gridOverlapWorldUnits, 0f, terrainTilePos.y * groundWidth -  groundWidth / 2f - gridOverlapWorldUnits))
        };

        // creates grid tiles        
        float yS = corners[0].y;
        float yE = corners[1].y;
        for (float y = yS; y <= yE; y++)
        {
            float xS = corners[0].x;
            float xE = corners[1].x;
            //if the offset row sticks up, reduce the number of hexes in a row
            /*if (y % 2 != 0 && (extendedGridSize.x + 0.5) * hexWidth > groundWidth)
                sizeX--;*/
            for (float x = xS; x <= xE; x++)
            {
                Tile tile;
                Point location = new Point((int)x - (int)Math.Floor(y / 2), (int)y);
                // if we're looking at an overlap tile and we already have the tile created then we don't need to create it anymore
                if ( board.ContainsKey(location)) // (y == yS || x == xS || y == yE || x == xE) &&
                {
                    tile = board[location];
                }
                else
                {
                    GameObject hex = Instantiate(Hex);
                    hex.transform.position = calcWorldCoord(new Vector2(location.X, location.Y));
                    hex.transform.parent = hexGridContainer.transform;

                    //y / 2 is subtracted from x because we want to go from using wiggly axis coordinate system to using straight axial coordinates. 
                    tile = new Tile(location.X, location.Y);
                    tile.Representation = hex;
                    board.Add(tile.Location, tile);
                }
                affectedBoardTiles.Add(tile.Location, tile);
            }
        }
        
        //Neighboring tile coordinates of all the tiles are calculated
        foreach (Tile tile in affectedBoardTiles.Values)
            tile.FindNeighbours(board);

        return affectedBoardTiles;
    }

    //Distance between destination tile and some other tile in the grid (TODO: in euclidean distance? or grid coords)
    public double CalcDistance(Tile tile, Tile destTile)
    {
        //Formula used here can be found in Chris Schetter's article
        float deltaX = Mathf.Abs(destTile.X - tile.X);
        float deltaY = Mathf.Abs(destTile.Y - tile.Y);
        int z1 = -(tile.X + tile.Y);
        int z2 = -(destTile.X + destTile.Y);
        float deltaZ = Mathf.Abs(z2 - z1);

        return Mathf.Max(deltaX, deltaY, deltaZ);
    }

    private void DrawPath(IEnumerable<Tile> path)
    {
        if (this.path == null)
            this.path = new List<GameObject>();
        //Destroy game objects which used to indicate the path
        this.path.ForEach(Destroy);
        this.path.Clear();

        //Lines game object is used to hold all the "Line" game objects indicating the path
        GameObject lines = GameObject.Find("Lines");
        if (lines == null)
            lines = new GameObject("Lines");
        foreach (Tile tile in path)
        {
            var line = (GameObject)Instantiate(Line);
            //calcWorldCoord method uses squiggly axis coordinates so we add y / 2 to convert x coordinate from straight axis coordinate system
            Vector2 gridPos = new Vector2(tile.X + (int)Math.Floor(tile.Y / 2d), tile.Y);
            line.transform.position = calcWorldCoordWiggly(gridPos);
            this.path.Add(line);
            line.transform.parent = lines.transform;
        }
    }

    public Path<Tile> generatePath(Vector2 start, Vector2 end)
    {
        //We assume that the distance between any two adjacent tiles is 1
        //If you want to have some mountains, rivers, dirt roads or something else which might slow down the player you should replace the function with something that suits better your needs
        Func<Tile, Tile, double> distance = (node1, node2) => 1;

        Tile startTile;
        Tile endTile;
        if (board.TryGetValue(new Point((int)start.x, (int)start.y), out startTile) && board.TryGetValue(new Point((int)end.x, (int)end.y), out endTile))
        {
            var path = PathFinder.FindPath(startTile, endTile, distance, CalcDistance);
            //DrawPath(path)
            return path;
        }
        return null;
    }

    /// <summary>
    /// Returns an enemy entities on the target tile, if any exist
    /// </summary>
    /// <param name="myPlayer"></param>
    public IGameUnit[] GetEnemiesOnTile(Player myPlayer, Tile tile)
    {
        return allUnits.Where(g => myPlayer.IsEnemy(g.OwnedBy) && g.GetComponent<CharacterMovement>().curTile == tile).ToArray();
    }

    /// <summary>
    /// Returns all units of Player on a tile.
    /// </summary>
    /// <param name="myPlayer"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public IGameUnit[] GetFriendlyUnitsOnTile(Player myPlayer, Tile tile)
    {
        return allUnits.Where(g => g.OwnedBy == myPlayer && g.GetComponent<CharacterMovement>().curTile == tile).ToArray();
    }

    private Color hex_default_color = new Color(1, 1, 1, 85f/255f);

    // Update is called once per frame
    void Update()
    {
		var es = UnityEngine.EventSystems.EventSystem.current;
        // if the mouse isn't hovering over the gui
        //if (!worldCorners.Any(wc => isMouseOverUI(wc, Input.mousePosition)))
		if (es != null && es.IsPointerOverGameObject())
			return;
		
		//get mouse pos
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (LevelCreator.instance.Generator.Raycast(ray, out hit, 100f))
		{
			var coord = calcGridCoordStraightAxis(hit.point);
			Tile newSelectedTile;
			if (board.TryGetValue(new Point((int)Math.Round(coord.x), (int)Math.Round(coord.y)), out newSelectedTile))
			{
				//Debug.Log(newSelectedTile.ToString());
				if (!Village.InBuildMode)
				{
					if (selectedTile != null && selectedTile != newSelectedTile) selectedTile.Representation.GetComponent<SpriteRenderer>().color = hex_default_color;
					selectedTile = newSelectedTile;
                    var selectedTileRenderer = selectedTile.Representation.GetComponent<SpriteRenderer>();
                    selectedTileRenderer.color = Color.yellow;

					if (Input.GetMouseButton(1))
					{
                        if (selectedUnit != null)
                        {
                            var hasEnemy = GetEnemiesOnTile(selectedUnit.GetComponent<IGameUnit>().OwnedBy, selectedTile).FirstOrDefault();
                            selectedUnit.GetComponent<CharacterMovement>().SuggestMove(coord);
                            // if there's an enemy make color red
                            if (hasEnemy != null)
                                selectedTileRenderer.color = Color.red;
                        }
					}
					// the user wants to move there
					else if (Input.GetMouseButtonUp(1))
					{
						if (selectedUnit != null)
						{
							selectedUnit.GetComponent<IGameUnit>().CancelActions();
							selectedUnit.GetComponent<CharacterMovement>().MoveTo(coord);
						}
					}
					// does the user want to select something?
					else if (Input.GetMouseButtonDown(0))
					{
						GameObject selected = null;

						//get all units from that tile
						var entitiesOnTile = allUnits.Where(u => u.GetComponent<CharacterMovement>().curTile == selectedTile).Select(u => u.gameObject);
						entitiesOnTile = entitiesOnTile.Union(allBuildings.Where(b => b.GetComponent<ProductionGameBuilding>().Location == selectedTile));
						if (entitiesOnTile.Count() > 0)
						{
							selected = entitiesOnTile.First();
							entitiesOnTile.First().GetComponent<AbstractCommandableEntity>().Select();
						}

						// If nothing is selected we'll leave the current unit selected, but we can get rid of the building!
						// There will always be a unit selected, if possible
						if (selected == null)
						{
							//UnitPanelUI.instance.SetUnitPanelInfo(null);
							BuildingPanelUI.instance.SetBuildItems(null, 0);
							selectedBuilding = null;
							if (selectedUnit != null) selectedUnit.GetComponent<AbstractCommandableEntity>().Select();
						}
						else if (allBuildings.Contains(selected))
						{
							UnitPanelUI.instance.SetUnit(null);
						}
						else
						{
							BuildingPanelUI.instance.SetBuildItems(null, 0);
						}
					}
				}
				else
				{
					Village.BuildModeHoveredTile = newSelectedTile;
				}
			}
			else
			{
				//Debug.Log("out of range");
				//Debug.Log("coord.x " + coord.x.ToString() + ", coord.y " + coord.y.ToString());
			}
		}
		else
		{
			//Debug.Log("Raycast fail");
		}
    }


    void Start()
    {
        setSizes();
        hexGridContainer = new GameObject("HexGrid");

        worldCorners = uiElements.Select(uie => { Vector3[] wc = new Vector3[4];  uie.GetWorldCorners(wc); return wc; } ).ToList();

        AreaOfControlContainer = new GameObject("AreaOfControlContainer");
        //AreaOfControlContainer.transform.parent = gameObject.transform;
        UncoveredAreaOfControl = board.Values.ToList();

        foreach (var t in board.Values)
        {
            t.Uncover();
        }

        EventManager.StartListening("NextRound", nextRoundListener);
    }

    public GameObject Spawn(GameObject entity, Vector2 position)
    {
        var PC = _spawnCreate(entity);
        var cm = PC.GetComponent<CharacterMovement>();
        cm.setPos(position);
        return PC;
    }

    public GameObject Spawn(GameObject entity, Tile position)
    {
        var PC = _spawnCreate(entity);
        var cm = PC.GetComponent<CharacterMovement>();
        cm.setPos(position);
        return PC;
    }

    private GameObject _spawnCreate(GameObject entity)
    {
        GameObject PC = Instantiate(entity);
        // directly added to allUnits, 'cause otherwise there are units which don't profit from research additions (created on same round but not yet awoken --> no new action for them)
        allUnits.Add(PC.GetComponent<IGameUnit>());
        return PC;
    }

    /// <summary>
    /// Remove the fog from a list of tiles
    /// </summary>
    /// <param name="tiles"></param>
    public void UncoverTiles(List<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            tile.Uncover();
        }
    }

    private List<Tile> _playerViewCache;
    public List<Tile> GetTilesInPlayerView()
    {
        if (_playerViewCache == null)
        {
            var visibleTiles = allUnits.Where(u => u.OwnedBy == GameManager.instance.LocalPlayer)
                .SelectMany(u => GetHexArea(u.GetComponent<CharacterMovement>().curTile, u.GetComponent<CharacterMovement>().SightRadius)).ToList();
            visibleTiles.AddRange(allBuildings.Where(b => b.GetComponent<ProductionGameBuilding>() != null)
                .SelectMany(b => GetHexArea(b.GetComponent<ProductionGameBuilding>().Location, b.GetComponent<ProductionGameBuilding>().AreaOfControl)));
            _playerViewCache = visibleTiles.Distinct().ToList();
        }
        return _playerViewCache;
    }

    /*public GameObject DrawHexLine(Vector3[] edgepoints)
    {
        GameObject line = Instantiate(LineRendererLine);
        var lineComp = line.GetComponent<LineRenderer>();
        lineComp.SetPositions(edgepoints);
        return line;
    }*/

    // http://www.redblobgames.com/grids/hexagons/#range
    /*public List<Tile> GetHexAreaOLD(Tile center, int distance)
    {
        List<Tile> results = new List<Tile>();
        var cubecenter = hex_to_cube(new Vector2(center.X, center.Y));
        for (int dx = -distance; dx <= distance; dx++)
            for (int dy = Math.Max(-distance, -dx - distance); dy <= Math.Min(distance, -dx + distance); dy++)
            {
                var dz = -dx - dy;
                var pos = cube_to_hex(new Vector3(cubecenter.x + dx, cubecenter.y + dy, cubecenter.z + dz));
                Tile tile;
                if (board.TryGetValue(new Point((int)Math.Round(pos.x), (int)Math.Round(pos.y)), out tile))
                {
                    results.Add(tile);
                }
            }
        return results;
    }*/

    static readonly Dictionary<int, List<int[]>> axialGridAreaOffsets = new Dictionary<int, List<int[]>>
    {
        {
            1, new List<int[]> {
                new int[] { 0, 0 },
                new int[] { 1, 0 },
                new int[] { 1, -1 },
                new int[] { 0, -1 },
                new int[] { -1, 0 },
                new int[] { -1, +1 },
                new int[] { 0, 1 }
            }
        },
        {
            2, new List<int[]> {
                new int[] { 0, -2 },
                new int[] { 1, -2 },
                new int[] { 2, -2 },
                new int[] { 2, -1 },
                new int[] { 2, 0 },
                new int[] { 1, 1 },
                new int[] { 0, 2 },
                new int[] { -1, 2 },
                new int[] { -2, 2 },
                new int[] { -2, 1 },
                new int[] { -2, 0 },
                new int[] { -1, -1 }
            }
        },
        {
            3, new List<int[]> {
                new int[] { 0, -3 },
                new int[] { 1, -3 },
                new int[] { 2, -3 },
                new int[] { 3, -3 },
                new int[] { 3, -2 },
                new int[] { 3, -1 },
                new int[] { 3, 0 },
                new int[] { 2, 1 },
                new int[] { 1, 2 },
                new int[] { 0, 3 },
                new int[] { -1, 3 },
                new int[] { -2, 3 },
                new int[] { -3, 3 },
                new int[] { -3, 2 },
                new int[] { -3, 1 },
                new int[] { -3, 0 },
                new int[] { -2, -1 },
                new int[] { -1, -2 }
            }
        },
		{
			4, new List<int[]> {
				new int[] { 0, -4 },
				new int[] { 1, -4 },
				new int[] { 2, -4 },
				new int[] { 3, -4 },
				new int[] { 4, -4 },
				new int[] { 4, -3 },
				new int[] { 4, -2 },
				new int[] { 4, -1 },
				new int[] { 4, 0 },
				new int[] { 3, 1 },
				new int[] { 2, 2 },
				new int[] { 1, 3 },
				new int[] { 0, 4 },
				new int[] { -1, 4 },
				new int[] { -2, 4 },
				new int[] { -3, 4 },
				new int[] { -4, 4 },
				new int[] { -4, 3 },
				new int[] { -4, 2 },
				new int[] { -4, 1 },
				new int[] { -4, 0 },
				new int[] { -3, -1 },
				new int[] { -2, -2 },
				new int[] { -1, -3 }
			}
		}
    };

    public List<Tile> GetHexArea(Tile center, int distance)
    {
        List<Tile> results = new List<Tile>();
        // sum up all affected tiles according to offset dictionary
        for (int i = 1; i <= distance; i++)
        {
            foreach (var offset in axialGridAreaOffsets[i])
            {
                Tile tile;
                if (board.TryGetValue(new Point(center.X + offset[0], center.Y + offset[1]), out tile))
                {
                    results.Add(tile);
                }
            }
        }
        return results;
    }

    public List<Tile> DrawHexArea(Tile center, int distance, int sprite, Tile.TileColorPresets color)
    {
        var area = GetHexArea(center, distance);
        foreach (var tile in area)
            tile.SetLooks(sprite, color);
        return area;
    }

    private GameObject AreaOfControlContainer;
    private List<Tile> UncoveredAreaOfControl;

    public void DrawAreaOfControl(Tile center, int distance)
    {
        AreaOfControlContainer.SetActive(true);
        var lowAlphaCol = new Color(0f, 0f, 0f, 0.6f);
        // cover all uncovered tiles
        foreach (var tile in UncoveredAreaOfControl)
        {
            GameObject cover = tile.AreaCover;
            //create area cover
            if (cover == null)
            {
                cover = Instantiate(HexFog);
                cover.transform.localScale = new Vector3(1f, 1f, 1f);
                var spr = cover.GetComponent<SpriteRenderer>();
                spr.sprite = tileSprites[2];
                spr.color = lowAlphaCol;
                cover.transform.position = new Vector3(tile.Representation.transform.position.x, cover.transform.position.y, tile.Representation.transform.position.z);
                cover.transform.parent = AreaOfControlContainer.transform; //tile.Representation.transform;
                tile.AreaCover = cover;
            }
            cover.SetActive(true);
        }
        // uncover hex area
        var area = GetHexArea(center, distance);
        foreach (var tile in area)
        {
            if (tile.AreaCover != null)
                tile.AreaCover.SetActive(false);
        }
        UncoveredAreaOfControl = area;
    }

    public void HideAreaOfControl()
    {
        AreaOfControlContainer.SetActive(false);
    }

    private void nextRoundListener()
    {
        _playerViewCache = null;
    }
}