using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace TerrainGenerator
{
    // Chunk management inspired majorly by http://code-phi.com/infinite-terrain-generation-in-unity-3d/
    public class TerrainChunkGenerator
    {
        public Material TerrainMaterial;

        private TerrainChunkSettings Settings;

        private NoiseProvider NoiseProvider;

        private ChunkCache Cache;

        public TerrainChunkGenerator()
        {
            Settings = new TerrainChunkSettings(257, 256, GridManager.instance.groundWidth, 2); //  / 40f * 40.15f
            NoiseProvider = new NoiseProvider();

            Cache = new ChunkCache();
        }

        public void Update()
        {
            Cache.Update();
        }

        private void GenerateChunk(int x, int z)
        {
            if (Cache.ChunkCanBeAdded(x, z))
            {
                Debug.Log("Cache: Generate Chunk " + x + ", " + z);
                var chunk = new TerrainChunk(Settings, NoiseProvider, x, z);
                Cache.AddNewChunk(chunk);
            }
        }

        private void RemoveChunk(int x, int z)
        {
            if (Cache.ChunkCanBeRemoved(x, z))
                Cache.RemoveChunk(x, z);
        }

        private List<Vector2i> GetChunkPositionsAround(Vector2i chunkPosition)
        {
            var result = new List<Vector2i>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    result.Add(new Vector2i(chunkPosition.X + i, chunkPosition.Z + j));
                }
            }

            return result;
        }

        /// <summary>
        /// Updates which chunks to generate and which to remove
        /// </summary>
        public void UpdateTerrain(IEnumerable<Vector3> visibilityPositions)
        {
            var newPositions = visibilityPositions.Select(p => GetChunkPosition(p)).Distinct().ToList();
            newPositions = newPositions.SelectMany(wp => GetChunkPositionsAround(wp)).Distinct().ToList();
            //Debug.Log("Chunks still needed: " + string.Join(", ", newPositions.Where(p => !Cache.IsChunkGenerated(p)).Select(p => p.ToString()).ToArray()) + ", Total: (" + string.Join(", ", newPositions.Select(p => p.ToString()).ToArray()) + ")");
            
            /*var newPositions = visibilityPositions.Select(p => GetChunkPosition(p)).ToList();
            newPositions.Add(new Vector2i(0, 1));
            newPositions.Add(new Vector2i(-1, 1));
            newPositions.Add(new Vector2i(-2, 2));
            newPositions.Add(new Vector2i(-3, 3));
            newPositions.Add(new Vector2i(0, -1));
            newPositions.Add(new Vector2i(1, -1));
            newPositions.Add(new Vector2i(0, -2));
            newPositions.Add(new Vector2i(1, -2));
            newPositions.Add(new Vector2i(2, -2));
            newPositions.Add(new Vector2i(0, -3));
            newPositions.Add(new Vector2i(1, -3));
            newPositions.Add(new Vector2i(2, -3));
            newPositions.Add(new Vector2i(3, -3));
            newPositions.Add(new Vector2i(2, -4));
            newPositions.Add(new Vector2i(3, -4));
            newPositions.Add(new Vector2i(4, -4));*/

            var loadedChunks = Cache.GetGeneratedChunks();
            var chunksToRemove = loadedChunks.Except(newPositions).ToList();

            var positionsToGenerate = newPositions.Except(chunksToRemove).ToList();
            foreach (var position in positionsToGenerate)
                GenerateChunk(position.X, position.Z);

            foreach (var position in chunksToRemove)
                RemoveChunk(position.X, position.Z);
        }

        public Vector2i GetChunkPosition(Vector3 worldPosition)
        {
            int x = worldPosition.x != 0f ? (int)Mathf.Floor(worldPosition.x / Settings.Length) : 0;
            int z = worldPosition.y != 0f ? (int)Mathf.Floor(worldPosition.z / Settings.Length) : 0;

            return new Vector2i(x, z);
        }

        public bool Raycast(Ray ray, out RaycastHit hit, float maxDistance)
        {
            foreach (var terrain in Cache.GetAllGeneratedChunkTerrains())
                if (terrain != null && terrain.Terrain != null && terrain.Terrain.GetComponent<Collider>().Raycast(ray, out hit, maxDistance))
                    return true;
            hit = new RaycastHit();
            return false;
        }

        public bool IsTerrainAvailable(Vector3 worldPosition)
        {
            var chunkPosition = GetChunkPosition(worldPosition);
            return Cache.IsChunkGenerated(chunkPosition);
        }

        public float GetTerrainHeight(Vector3 worldPosition)
        {
            var chunkPosition = GetChunkPosition(worldPosition);
            var chunk = Cache.GetGeneratedChunk(chunkPosition);
            if (chunkPosition != null)
                return chunk.GetTerrainHeight(worldPosition);

            return 0;
        }
    }
}