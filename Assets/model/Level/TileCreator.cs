using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TerrainGenerator;
using UnityEngine;

namespace Assets.model.Level
{
    // TODO: Use Mathf.PerlinNoise?
    public class TileCreator
    {
        // Determines the order of tile types with their cutoff values (e.g. River will be on tile if perlinval <= 0.25f)
        private static Dictionary<Tile.TerrainType, float> tiletypeChances = new Dictionary<Tile.TerrainType, float>()
        {
            { Tile.TerrainType.RIVER, 0.25f },
            { Tile.TerrainType.FOREST, 0.4f },
            { Tile.TerrainType.DRYEARTH, 0.2f },
            { Tile.TerrainType.REDSTONE, 0.05f }
        };
        private static Tile.TerrainType defaultType = Tile.TerrainType.GRASS;
        // perlin noise scales per tile type
        readonly static float[] noiseScales = new float[4]
        {
            0.04f,
            0.2f,
            0.05f,
            0.05f
        };
        const float shift = 20f;
        public static int Seed;

        public static void CreateFromNoise(Dictionary<Point, Tile> board, Vector2i terrainTilePos, Vector3 terrainWorldPos)
        {
            var unassignedBoardTiles = board.Values.Where(t => t.Type == Tile.TerrainType.UNASSIGNED).ToList();
            
            foreach (var tile in unassignedBoardTiles)
            {
                //var xCoordinate = Position.X + (float)xRes / (Settings.HeightmapResolution - 1);
                //var zCoordinate = Position.Z + (float)zRes / (Settings.HeightmapResolution - 1);
                //heightmap[zRes, xRes] = NoiseProvider.GetValue(xCoordinate, zCoordinate);

                for (int i = 0; i < tiletypeChances.Count; i++)
                {
                    if (1f - Mathf.PerlinNoise(tile.X * noiseScales[i] + shift * i + Seed, tile.Y * noiseScales[i] + shift * i + Seed) <= tiletypeChances.Values.ElementAt(i))
                    {
                        tile.SetTerrainType(tiletypeChances.Keys.ElementAt(i));
                        break;
                    }
                }

                // set default type if height maps didn't lead to result
                if (tile.Type == Tile.TerrainType.UNASSIGNED)
                    tile.SetTerrainType(defaultType);

                //assign strategic resources
                var resourceNoise = IntegerNoise(Seed * 100 + tile.X + tile.Y * 1000000);
                var matchingResources = LevelCreator.instance.StrategicResources.Where(s => s.RequiredType == tile.Type);
                foreach (var resource in matchingResources)
                {
                    if (resourceNoise < resource.ChanceOfAppearal)
                    {
                        // we have found our resource
                        tile.SetStrategicResource(resource);
                        break;
                    }
                    resourceNoise -= resource.ChanceOfAppearal;
                }
            }
        }

        // Returns a noise value from 0 to 1 (deterministic but uncorrelated) for any int value, uniformly distributed
        // http://libnoise.sourceforge.net/noisegen/index.html#coherentnoise
        private static double IntegerNoise(int n)
        {
            n = (n >> 13) ^ n;
            int nn = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
            return (1.0 - ((double)nn / 1073741824.0)) / 2d + 0.5;
        }

        public static void Create(Dictionary<Point, Tile> board, Vector2i terrainTilePos, Vector3 terrainWorldPos, NoiseProvider noiseProvider)
        {
            CreateFromNoise(board, terrainTilePos, terrainWorldPos);
            return;
            /*var heightmap = new float[Settings.HeightmapResolution, Settings.HeightmapResolution];

            for (var zRes = 0; zRes < Settings.HeightmapResolution; zRes++)
            {
                for (var xRes = 0; xRes < Settings.HeightmapResolution; xRes++)
                {
                    var xCoordinate = Position.X + (float)xRes / (Settings.HeightmapResolution - 1);
                    var zCoordinate = Position.Z + (float)zRes / (Settings.HeightmapResolution - 1);

                    heightmap[zRes, xRes] = NoiseProvider.GetValue(xCoordinate, zCoordinate);
                }
            }*/

            Debug.Log("CP1 of " + terrainTilePos.ToString());

            var gm = GridManager.instance;

            Debug.Log("CP2 of " + terrainTilePos.ToString());
            var boardValues = new List<Tile>(board.Values);
            boardValues.Shuffle();

            Debug.Log("CP3 of " + terrainTilePos.ToString());
            // initialize 1 - 2 redstone fields
            for (var i = 0; i < LevelCreator.Random.Next(2, 3); i++)
            {
                var el = boardValues.First(b => b.Type == Tile.TerrainType.UNASSIGNED);
                Debug.Log("CP3.1 of " + terrainTilePos.ToString());
                el.SetTerrainType(Tile.TerrainType.REDSTONE);
                var rsUnassigned = el.AllNeighbours.Where(e => e.Type == Tile.TerrainType.UNASSIGNED).ToArray();
                if (rsUnassigned.Count() == 0) continue;
                var rs = rsUnassigned.ElementAt(LevelCreator.Random.Next(0, rsUnassigned.Count() - 1));
                Debug.Log("CP3.2 of " + terrainTilePos.ToString());
                rs.SetTerrainType(Tile.TerrainType.REDSTONE);
                boardValues.Remove(rs);
                Debug.Log("CP3.3 of " + terrainTilePos.ToString());
            }
            Debug.Log("CP4 of " + terrainTilePos.ToString());

            int amountRedstone = board.Values.Count(tl => tl.Type == Tile.TerrainType.REDSTONE);

            // keep check on strategic resources
            Dictionary<StrategicResource, int> placedResources = new Dictionary<StrategicResource, int>();
            //foreach (var res in LevelCreator.instance.StrategicResources)
                //placedResources.Add(res, LevelCreator.Random.Next(res.MinAmount, res.MaxAmount));

            Debug.Log("CP5 of " + terrainTilePos.ToString());

            // create river
            var riverTiles = new List<Tile>();
            riverTiles.Add(boardValues.First(b => b.Type == Tile.TerrainType.UNASSIGNED));
            riverTiles.Last().SetTerrainType(Tile.TerrainType.RIVER);
            for (int i = 0; i < 10; i++)
            {
                var r = riverTiles.Last();
                var r_nb = new List<Tile>(r.AllNeighbours);
                r_nb.Shuffle();
                Tile r2 = null;
                while (r2 == null && r_nb.Count > 0)
                {
                    var random_nb = r_nb.ElementAt(LevelCreator.Random.Next(0, r_nb.Count - 1));
                    if (random_nb.Type == Tile.TerrainType.UNASSIGNED)
                    {
                        r2 = random_nb;
                    }
                    else
                    {
                        r_nb.Remove(random_nb);
                    }
                }
                if (r2 == null) break;
                r2.SetTerrainType(Tile.TerrainType.RIVER);
                riverTiles.Add(r2);
                LevelCreator.TryPlaceMatchingResource(placedResources, r2, gm.calcWorldCoord(new Vector2(r2.Location.X, r2.Location.Y)));
            }
            

            foreach (var tile in boardValues)
            {
                if (tile.Type == Tile.TerrainType.UNASSIGNED)
                {
                    var tilecenter = gm.calcWorldCoord(new Vector2(tile.Location.X, tile.Location.Y));

                    var surroundingTypes = tile.Neighbours.Select(tl => tl.Type).ToArray();

                    int amountSurroundingRedstone = surroundingTypes.Count(tl => tl == Tile.TerrainType.REDSTONE);

                    if (amountSurroundingRedstone >= 2 && amountRedstone <= 30 && LevelCreator.Random.NextDouble() <= 0.5d + 1d / amountRedstone)
                    {
                        tile.SetTerrainType(Tile.TerrainType.REDSTONE);
                        amountRedstone++;
                    }
                    else
                    {
                        if (LevelCreator.Random.NextDouble() < 0.1)
                        {
                            //forest
                            tile.SetTerrainType(Tile.TerrainType.FOREST);
                        }
                        else
                        {
                            tile.SetTerrainType(Tile.TerrainType.GRASS);

                        }
                    }

                    LevelCreator.TryPlaceMatchingResource(placedResources, tile, tilecenter);
                }
            }
            

            //Debug.Log("Tree x min max:" + terrainData.treeInstances.Min(ti => ti.position.x) + " " + terrainData.treeInstances.Max(ti => ti.position.x));
            //Debug.Log("Tree z min max:" + terrainData.treeInstances.Min(ti => ti.position.z) + " " + terrainData.treeInstances.Max(ti => ti.position.z));

            // Like this: Iterate through every row
            /*for (int y = 0; y < gridSize.y; y++)
            {
                float sizeX = gridSize.x;
                //if the offset row sticks up, reduce the number of hexes in a row
                if (y % 2 != 0 && (gridSize.x + 0.5) * gm.hexWidth > gm.groundWidth)
                    sizeX--;
                for (float x = 0; x < sizeX; x++)
                {

                }
            }*/

            var amountUnassigned = board.Values.Count(tl => tl.Type == Tile.TerrainType.UNASSIGNED);
            if (amountUnassigned > 0)
            {
                throw new UnassignedReferenceException(amountUnassigned + " tiles have no type assigned!");
            }
            
        }
    }
}
