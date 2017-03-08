using System.Collections;
using TerrainGenerator;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private const int Radius = 4;

    private Vector2i PreviousPlayerChunkPosition;

    public Transform Player;

    public TerrainChunkGenerator Generator;

    public Button StartButton;

    /*public void StartAll()
    {
        StartCoroutine(InitializeCoroutine());
    }

    private IEnumerator InitializeCoroutine()
    {
        var canActivateCharacter = false;

        StartButton.interactable = false;
        Generator.UpdateTerrain(Player.position, Radius);

        do
        {
            var exists = Generator.IsTerrainAvailable(Player.position);
            if (exists)
                canActivateCharacter = true;
            yield return null;
        } while (!canActivateCharacter);

        PreviousPlayerChunkPosition = Generator.GetChunkPosition(Player.position);
        Player.position = new Vector3(Player.position.x, Generator.GetTerrainHeight(Player.position) + 0.5f, Player.position.z);
        Player.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Player.gameObject.activeSelf)
        {
            var playerChunkPosition = Generator.GetChunkPosition(Player.position);
            if (!playerChunkPosition.Equals(PreviousPlayerChunkPosition))
            {
                Generator.UpdateTerrain(Player.position, Radius);
                PreviousPlayerChunkPosition = playerChunkPosition;
            }
        }
    }*/
}