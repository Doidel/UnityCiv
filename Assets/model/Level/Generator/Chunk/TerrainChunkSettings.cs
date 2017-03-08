using UnityEngine;

namespace TerrainGenerator
{
    public class TerrainChunkSettings
    {
        public readonly int HeightmapResolution;

        public readonly int AlphamapResolution;

        public readonly float Length;

        public readonly float Height;

        public TerrainChunkSettings(int heightmapResolution, int alphamapResolution, float length, float height)
        {
            HeightmapResolution = heightmapResolution;
            AlphamapResolution = alphamapResolution;
            Length = length;
            Height = height;
        }
    }
}