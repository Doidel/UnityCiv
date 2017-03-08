using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerator;

public class WildAnimalSpawner : MonoBehaviour {

    public WildAnimal[] Animals;

    private int animalAmountPerChunk = 5;
    private List<WildAnimal> SpawnedAnimals = new List<WildAnimal>();

	// Use this for initialization
	void Start () {
	
	}

    void OnEnable()
    {
        EventManager.StartListening("NextRound", nextRoundListener);
    }

    void OnDisable()
    {
        EventManager.StopListening("NextRound", nextRoundListener);
    }
    
    private void nextRoundListener()
    {
        // We have to make sure that there are animals where player gameobjects are.
        // solution: For each terrain chunk where the player is, spawn objects where he doensn't see them. Otherwise despawn.
        var tilesInPlayerView = GridManager.instance.GetTilesInPlayerView();
        var relevantpositions = tilesInPlayerView.Select(t => GridManager.instance.calcWorldCoord(new Vector2(t.X, t.Y))).ToArray();
        //var relevantpositions = GridManager.instance.allUnits.Where(u => u.OwnedBy == GameManager.instance.LocalPlayer).Select(u => u.transform.position).Union(GridManager.instance.allBuildings.Select(u => u.transform.position));
        var relevantChunks = relevantpositions.Select(p => LevelCreator.instance.Generator.GetChunkPosition(p)).Distinct().ToList();

        var animalChunkPos = SpawnedAnimals.ToDictionary(s => s, a => LevelCreator.instance.Generator.GetChunkPosition(a.transform.position));

        // despawn all animals on the irrelevant chunks
        var animalsOutside = SpawnedAnimals.Where(a => !relevantChunks.Contains(animalChunkPos[a])).ToArray();
        foreach (var animal in animalsOutside)
        {
            SpawnedAnimals.Remove(animal);
            animalChunkPos.Remove(animal);
        }


        var random = new System.Random();

        foreach (var chunk in relevantChunks)
        {
            int animalsToSpawn = animalAmountPerChunk - animalChunkPos.Count(a => a.Value == chunk);
            if (animalsToSpawn > 0)
            {
                // spawn animals outside the player view
                for (int i = 0; i < animalsToSpawn; i++)
                {
                    var spawnTile = RandomTileForChunk(chunk, tilesInPlayerView, random);
                    if (spawnTile != null)
                    {
                        var newAnimal = GridManager.instance.Spawn(Animals[UnityEngine.Random.Range(0, Animals.Length - 1)].gameObject, spawnTile);
                        SpawnedAnimals.Add(newAnimal.GetComponent<WildAnimal>());
                    }
                    // TODO: Cache impossible to spawn, to save big check after every round
                }
            }
        }
    }

    private Tile RandomTileForChunk(Vector2i chunk, List<Tile> tilesInPlayerView, System.Random random)
    {
        var chunkPosX1 = chunk.X * GridManager.instance.groundWidth;
        var chunkPosX2 = (chunk.X + 1) * GridManager.instance.groundWidth;
        var chunkPosZ1 = chunk.Z * GridManager.instance.groundWidth;
        var chunkPosZ2 = (chunk.Z + 1) * GridManager.instance.groundWidth;

        // try to find a spot 5 times
        for (int i = 0; i < 5; i++)
        {
            var randomPos = new Vector2((float)random.NextDouble() * (chunkPosX2 - chunkPosX1) + chunkPosX1, (float)random.NextDouble() * (chunkPosZ2 - chunkPosZ1) + chunkPosZ1);
            var coord = GridManager.instance.calcGridCoordStraightAxis(randomPos);
            Tile spawnTile;
            if (GridManager.instance.board.TryGetValue(new Point((int)Math.Round(coord.x), (int)Math.Round(coord.y)), out spawnTile))
            {
                if (!tilesInPlayerView.Contains(spawnTile))
                {
                    return spawnTile;
                }
            }
        }

        // otherwise just take a neighbouring tile which isn't in view
        foreach (var tile in tilesInPlayerView)
        {
            var tileWorldCoord = GridManager.instance.calcWorldCoord(new Vector2(tile.X, tile.Y));
            if (tileWorldCoord.x < chunkPosX1 || tileWorldCoord.x > chunkPosX2 || tileWorldCoord.z < chunkPosZ1 || tileWorldCoord.z > chunkPosZ2)
                continue;

            var spawnTile = tile.AllNeighbours.FirstOrDefault(t => !tilesInPlayerView.Contains(t));
            if (spawnTile != null)
                return spawnTile;
        }
        return null;
    }
}
