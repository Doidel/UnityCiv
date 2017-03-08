using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Assets.model.Level;
using TerrainGenerator;
using System.Threading;

public class LevelCreator : MonoBehaviour {

    public GameObject TerrainPrefab;
    public GameObject WaterPrefab;
    [HideInInspector]
    public TerrainChunkGenerator Generator;
    private const float treePlacementTolerance = 0.01f;
    [HideInInspector]
    public StrategicResource[] StrategicResources;

    /// <summary>
    /// THE big random seed for level creation
    /// </summary>
    public static System.Random Random = new System.Random();

    public static void TryPlaceMatchingResource(Dictionary<StrategicResource, int> placedResources, Tile tile, Vector3 tilecenter)
    {
        var matchingResource = placedResources.Keys.FirstOrDefault(f => placedResources[f] > 0 && f.RequiredType == tile.Type);
        if (matchingResource != null)
        {
            tile.SetStrategicResource(matchingResource);
            placedResources[matchingResource]--;
        }
    }

    public static void PlaceTreesOnTile(System.Random rnd, Terrain t, Vector3 worldpos, TerrainChunkSettings settings, Vector3 tilecenter)
    {
        var amountOfTreesOnTile = rnd.Next(9, 14);
        float r = Math.Min(GridManager.instance.hexWidth, GridManager.instance.hexHeight) / 2;
        List<float[]> existingOffsets = new List<float[]>();
        for (int i = 0; i < amountOfTreesOnTile; i++)
        {
            // find a place to plant the tree
            float xoffset = -99;
            float zoffset = -99;
            for (int placeTry = 0; placeTry < 10; placeTry++)
            {
                var rot = rnd.NextDouble() * 2 * Math.PI;
                float dist = (float)rnd.NextDouble() * r;
                var xo = (float)Math.Cos(rot) * dist;
                var zo = (float)Math.Sin(rot) * dist;
                // if there's not a tree already
                if (!existingOffsets.Any(e => (e[0] - xo) * (e[0] - xo) + (e[1] - zo) * (e[1] - zo) <= treePlacementTolerance))
                {
                    xoffset = xo;
                    zoffset = zo;
                    break;
                }
            }
            // no suitable tree position found
            if (xoffset == -99)
            {
                //Debug.Log("aborted");
                break;
            }

            var treepos = new Vector3((tilecenter.x - worldpos.x + xoffset) / (settings.Length), 0, (tilecenter.z - worldpos.z + zoffset) / (settings.Length));
            if (treepos.x < 0f || treepos.x > 1f || treepos.z < 0f || treepos.z > 1f)
            {
                break;
            }

            TreeInstance ti = new TreeInstance();
            ti.prototypeIndex = 0;
            ti.heightScale = 0.02f;
            ti.widthScale = 0.02f;
            ti.color = Color.white;
            ti.position = treepos;

            existingOffsets.Add(new float[] { xoffset, zoffset });
            t.AddTreeInstance(ti);
        }

        // place big tree in the middle
        /*TreeInstance treeDebug = new TreeInstance();
        treeDebug.prototypeIndex = 0;
        treeDebug.heightScale = 0.05f;
        treeDebug.widthScale = 0.03f;
        treeDebug.color = Color.white;
        treeDebug.position = new Vector3((tilecenter.x - worldpos.x) / (settings.Length * settings.Scale), 0, (tilecenter.z - worldpos.z) / (settings.Length * settings.Scale));
        t.AddTreeInstance(treeDebug);*/
    }
    
    public static void PlaceGameObjectsOnTiles(Dictionary<Point, Tile> boardTiles, Terrain t, TerrainChunkSettings settings, Vector3 terrainWorldPos)
    {
        foreach (var tile in boardTiles.Values)
        {
            var tilecenter = GridManager.instance.calcWorldCoord(new Vector2(tile.Location.X, tile.Location.Y));
            //tilecenter.x -= tile.Location.Y % 2 != 0 ? 0.5f * GridManager.instance.hexWidth : 0f;

            // plant trees if it's a forest
            if (tile.Type == Tile.TerrainType.FOREST)
            {
                PlaceTreesOnTile(Random, t, terrainWorldPos, settings, tilecenter);
            }

            //place the gameobject for the strategic resource if there's any
            if (tile.StrategicResource != null)
            {
                GameObject res = Instantiate(tile.StrategicResource.Model);
                res.transform.position = tilecenter;
                res.hideFlags = HideFlags.HideInHierarchy;
            }
        }
    }

    public static LevelCreator instance = null;

    void Awake()
    {
        instance = this;

        TileCreator.Seed = (int)(Random.NextDouble() * 99999);

        StrategicResources = new StrategicResource[]
        {
            new StrategicResource()
            {
                Name = "Hen",
                BaseYieldFood = 1,
                Icon = Resources.Load<Sprite>("Icons/resourceicon_chicken"),
                ChanceOfAppearal = 0.03f,
                Model = Resources.Load<GameObject>("Prefabs/StrategicResource_Chicken"),
                RequiredType = Tile.TerrainType.GRASS,
                ResourceType = StrategicResource.Type.Farmable
            },
            new StrategicResource()
            {
                Name = "Deer",
                BaseYieldFood = 1,
                Icon = Resources.Load<Sprite>("Icons/resourceicon_deer"),
                ChanceOfAppearal = 0.015f,
                Model = Resources.Load<GameObject>("Prefabs/StrategicResource_Deer"),
                RequiredType = Tile.TerrainType.GRASS,
                ResourceType = StrategicResource.Type.Huntable
            },
            new StrategicResource()
            {
                Name = "Boar",
                BaseYieldFood = 1,
                Icon = Resources.Load<Sprite>("Icons/resourceicon_boar"),
                ChanceOfAppearal = 0.015f,
                Model = Resources.Load<GameObject>("Prefabs/StrategicResource_Boar"),
                RequiredType = Tile.TerrainType.GRASS,
                ResourceType = StrategicResource.Type.Huntable
            },
            new StrategicResource()
            {
                Name = "Fish",
                BaseYieldFood = 1,
                Icon = Resources.Load<Sprite>("Icons/resourceicon_fish"),
                ChanceOfAppearal = 0.04f,
                Model = Resources.Load<GameObject>("Prefabs/StrategicResource_Fish"),
                RequiredType = Tile.TerrainType.RIVER,
                ResourceType = StrategicResource.Type.Fishable
            }
        };
    }

    void Start()
    {
        Generator = new TerrainChunkGenerator();
        //StartCoroutine(InitializeCoroutine());

        Generator.UpdateTerrain(new List<Vector3>() { GridManager.instance.calcWorldCoord(new Vector2(0, 0)) });
        var settlerInstance = GridManager.instance.Spawn(GameManager.instance.Settler, new Vector2(0, 0));
        GameManager.instance.CameraControls.FlyToTarget(settlerInstance.transform.position);
    }

    float _secondsUpdate = 2f;
    void Update()
    {
        Generator.Update();
        

        // update terrain every 2 seconds
        _secondsUpdate += Time.deltaTime;
        if (_secondsUpdate > 2f)
        {
            //var relevantpositions = GridManager.instance.allUnits.Where(u => u.OwnedBy == GameManager.instance.LocalPlayer).Select(u => u.transform.position).Union(GridManager.instance.allBuildings.Select(u => u.transform.position));
            // TODO: Area the camera view spans
            var relevantpositions = new Vector3[] {
                ScreenPointToXZPlane(new Vector3(0, 0, 0)),
                ScreenPointToXZPlane(new Vector3(Screen.width, 0, 0)),
                ScreenPointToXZPlane(new Vector3(0, Screen.height, 0)),
                ScreenPointToXZPlane(new Vector3(Screen.width, Screen.height, 0))
            };
            Generator.UpdateTerrain(relevantpositions);
            _secondsUpdate-= 2f;
        }
    }


    static Plane XZPlane = new Plane(Vector3.up, Vector3.zero);
    public static Vector3 ScreenPointToXZPlane(Vector3 screenPoint)
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (XZPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            return hitPoint;
        }
        throw new InvalidOperationException("Screen to XZPlane is zero..?");
    }
}
