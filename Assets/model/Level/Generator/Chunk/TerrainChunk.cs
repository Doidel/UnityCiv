using Assets.model.Level;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TerrainGenerator
{
    public class TerrainChunk
    {
        public Vector2i Position { get; private set; }

        public Vector3 WorldPosition { get; private set; }

        public Dictionary<Point, Tile> Tiles { get; private set; }

        public Terrain Terrain { get; private set; }

        private TerrainData Data { get; set; }

        private TerrainChunkSettings Settings { get; set; }

        private NoiseProvider NoiseProvider { get; set; }

        private TerrainChunkNeighborhood Neighborhood { get; set; }

        private float[,] Heightmap { get; set; }

        private float[,,] Alphamap { get; set; }

        public TerrainChunk(TerrainChunkSettings settings, NoiseProvider noiseProvider, int x, int z)
        {
            //TODO: Overlapping tiles for different chunks crashes game
            Tiles = GridManager.instance.CreateOrGetGrid(new Vector2(x, z)); // creates the entire hex grid already for this thread, on the main thread. This consumes quite a bit of performance, there might be better ways...
            Settings = settings;
            NoiseProvider = noiseProvider;
            Neighborhood = new TerrainChunkNeighborhood();

            Position = new Vector2i(x, z);
            WorldPosition = new Vector3(Position.X * Settings.Length - Settings.Length / 2, -0.2f, Position.Z * Settings.Length - Settings.Length / 2);

            Debug.Log("Working on tile assigment  " + Position.X.ToString() + ", " + Position.Z.ToString());
            TileCreator.Create(Tiles, Position, WorldPosition, NoiseProvider); // due to overlapping tiles we currently do this here instead of in a thread
            Debug.Log("Finished  tile assigment " + Position.X.ToString() + ", " + Position.Z.ToString());
        }

        #region Heightmap stuff
        private Thread heightmapThread;
        private bool hasStarted = false;
        public void GenerateHeightmap()
        {
            heightmapThread = new Thread(GenerateHeightAndColormap);
            heightmapThread.Start();
            hasStarted = true;
            //GenerateHeightAndColormap();
        }

        public bool IsDone { get { return hasStarted && !heightmapThread.IsAlive; } }

        private void GenerateHeightAndColormap()
        {
                var res = TextureSplatPainter.Paint(WorldPosition, Tiles, Settings);
                Alphamap = res.Alphamap;
                Heightmap = res.Heightmap;
                Debug.Log("Finished  painting " + Position.X.ToString() + ", " + Position.Z.ToString());
        }

        public float GetTerrainHeight(Vector3 worldPosition)
        {
            return Terrain.SampleHeight(worldPosition);
        }

        #endregion

        #region Main terrain generation

        public void CreateTerrain()
        {
            /*var newTerrainGameObject = Instantiate(TerrainPrefab);
            Grounds.Add(newTerrainGameObject);
            newTerrainGameObject.transform.position = new Vector3(-20f + terrainTilePos.x * 40f, -0.2f, -20f + terrainTilePos.y * -40f);

            var t = newTerrainGameObject.GetComponent<Terrain>();
            t.terrainData = Instantiate<TerrainData>(t.terrainData);*/


            Data = new TerrainData();
            //Data = Object.Instantiate(LevelCreator.instance.TerrainPrefab.GetComponent<Terrain>().terrainData);
            var othertd = LevelCreator.instance.TerrainPrefab.GetComponent<Terrain>().terrainData;
            Data.treePrototypes = othertd.treePrototypes;
            Data.splatPrototypes = othertd.splatPrototypes;
            Data.treeInstances = new TreeInstance[] { };
            Data.heightmapResolution = Settings.HeightmapResolution;
            Data.alphamapResolution = Settings.AlphamapResolution;
            if (Alphamap == null || Heightmap == null) throw new UnityException("Map missing at " + Position);
            Data.SetAlphamaps(0, 0, Alphamap);
            Data.SetHeights(0, 0, Heightmap);
            //ApplyTextures(Data);

            var newTerrainGameObject = Terrain.CreateTerrainGameObject(Data);
            newTerrainGameObject.name = "Terrain " + Position.X + "/" + Position.Z;
            newTerrainGameObject.transform.position = WorldPosition;
            newTerrainGameObject.layer = 10;

            Terrain = newTerrainGameObject.GetComponent<Terrain>();
            Data.size = new Vector3(Settings.Length, Settings.Height, Settings.Length);
            Terrain.heightmapPixelError = 1;
            //Terrain.materialType = UnityEngine.Terrain.MaterialType.Custom;
            //Terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            LevelCreator.PlaceGameObjectsOnTiles(Tiles, Terrain, Settings, WorldPosition);
            Terrain.Flush();
            // Water
            var water = Object.Instantiate(LevelCreator.instance.WaterPrefab);
            water.transform.position = new Vector3(WorldPosition.x + Settings.Length / 2, -0.13f, WorldPosition.z + Settings.Length / 2);
            water.transform.parent = newTerrainGameObject.transform;
        }

        /*private void ApplyTextures(TerrainData terrainData)
        {
            var flatSplat = new SplatPrototype();
            var steepSplat = new SplatPrototype();

            flatSplat.texture = Settings.FlatTexture;
            steepSplat.texture = Settings.SteepTexture;

            terrainData.splatPrototypes = new SplatPrototype[]
            {
                flatSplat,
                steepSplat
            };

            terrainData.RefreshPrototypes();

            var splatMap = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, 2];

            for (var zRes = 0; zRes < terrainData.alphamapHeight; zRes++)
            {
                for (var xRes = 0; xRes < terrainData.alphamapWidth; xRes++)
                {
                    var normalizedX = (float)xRes / (terrainData.alphamapWidth - 1);
                    var normalizedZ = (float)zRes / (terrainData.alphamapHeight - 1);

                    var steepness = terrainData.GetSteepness(normalizedX, normalizedZ);
                    var steepnessNormalized = Mathf.Clamp(steepness / 1.5f, 0, 1f);

                    splatMap[zRes, xRes, 0] = 1f - steepnessNormalized;
                    splatMap[zRes, xRes, 1] = steepnessNormalized;
                }
            }

            terrainData.SetAlphamaps(0, 0, splatMap);
        }*/

        #endregion

        #region Distinction

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as TerrainChunk;
            if (other == null)
                return false;

            return this.Position.Equals(other.Position);
        }

        #endregion

        #region Chunk removal

        public void Remove()
        {
            Heightmap = null;
            Settings = null;

            if (Neighborhood.XDown != null)
            {
                Neighborhood.XDown.RemoveFromNeighborhood(this);
                Neighborhood.XDown = null;
            }
            if (Neighborhood.XUp != null)
            {
                Neighborhood.XUp.RemoveFromNeighborhood(this);
                Neighborhood.XUp = null;
            }
            if (Neighborhood.ZDown != null)
            {
                Neighborhood.ZDown.RemoveFromNeighborhood(this);
                Neighborhood.ZDown = null;
            }
            if (Neighborhood.ZUp != null)
            {
                Neighborhood.ZUp.RemoveFromNeighborhood(this);
                Neighborhood.ZUp = null;
            }

            if (Terrain != null)
                GameObject.Destroy(Terrain.gameObject);
        }

        public void RemoveFromNeighborhood(TerrainChunk chunk)
        {
            if (Neighborhood.XDown == chunk)
                Neighborhood.XDown = null;
            if (Neighborhood.XUp == chunk)
                Neighborhood.XUp = null;
            if (Neighborhood.ZDown == chunk)
                Neighborhood.ZDown = null;
            if (Neighborhood.ZUp == chunk)
                Neighborhood.ZUp = null;
        }

        #endregion

        #region Neighborhood

        public void SetNeighbors(TerrainChunk chunk, TerrainNeighbor direction)
        {
            if (chunk != null)
            {
                switch (direction)
                {
                    case TerrainNeighbor.XUp:
                        Neighborhood.XUp = chunk;
                        break;

                    case TerrainNeighbor.XDown:
                        Neighborhood.XDown = chunk;
                        break;

                    case TerrainNeighbor.ZUp:
                        Neighborhood.ZUp = chunk;
                        break;

                    case TerrainNeighbor.ZDown:
                        Neighborhood.ZDown = chunk;
                        break;
                }
            }
        }

        public void UpdateNeighbors()
        {
            if (Terrain != null)
            {
                var xDown = Neighborhood.XDown == null ? null : Neighborhood.XDown.Terrain;
                var xUp = Neighborhood.XUp == null ? null : Neighborhood.XUp.Terrain;
                var zDown = Neighborhood.ZDown == null ? null : Neighborhood.ZDown.Terrain;
                var zUp = Neighborhood.ZUp == null ? null : Neighborhood.ZUp.Terrain;
                Terrain.SetNeighbors(xDown, zUp, xUp, zDown);
                Terrain.Flush();
            }
        }

        #endregion
    }
}