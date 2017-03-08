using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    public AudioSource UIAudioSource;
    public CivCamera CameraControls;
    public Texture2D PointerNormal;
    public AudioClip Select1;
    public AudioClip Click1;
    public AudioClip MenuMusic;

    public GameObject TileValueDisplayPrefab;

    public GameObject Settler;
    

    [HideInInspector]
    public static List<BuildItem> AllBuildItems = new List<BuildItem>()
    {
        new BuildItem("Worker", "Icons/icon_worker", "Produce a worker", 10, 60, "Prefabs/Worker"),
        new BuildItem("Hunting Shack", "Icons/hunting_shack", "Build a hunting shack", 10, 10, "Prefabs/Building_HuntingShack"),
        new BuildItem("Fishing Hut", "Icons/fishinghut", "Build a fishing hut", 10, 10, "Prefabs/Building_Fishinghut"),
        new BuildItem("Scout", "Icons/icon_worker", "Produce a scout", 30, 30, "Prefabs/Scout"),
        new BuildItem("Hunter", "Icons/icon_worker", "Produce a hunter", 30, 30, "Prefabs/Hunter")
    };

    [HideInInspector]
    public static List<UnitAction> UnitActionPrefabs = new List<UnitAction>()
    {
        new UnitAction()
        {
            Name = "Sleep",
            Description = "Make no actions until awoken",
            ImageAssetPath = "Icons/btn_sleep"
        },
        new UnitAction()
        {
            Name = "BuildOutpost",
            Description = "Construct an outpost",
            Action = "Prefabs/Outpost",
            ImageAssetPath = "Icons/btn_outpost"
        }
    };

    public GameObject ScaffoldPrefab;
    

    public Research Research;

    public Player LocalPlayer { get; private set; }
    public Player WildAnimalsPlayer { get; private set; }

    public void PlayUIClick()
    {
        UIAudioSource.PlayOneShot(Click1, 0.3f);
    }

    public static GameManager instance = null;

    void Awake()
    {
        instance = this;

        Research = new Research();

        LocalPlayer = new Player("UnnamedPlayer");
        WildAnimalsPlayer = new Player("Wild Animals");
    }

    void Start()
    {
        // background music
        UIAudioSource.PlayOneShot(MenuMusic, 0.8f);

        Research.Start();
    }

    private bool tileResourcesDisplayed = false;
    public void ToggleResourceDisplay()
    {
        foreach (var t in GridManager.instance.board.Values)
        {
            if (t.InPlayerTerritory)
            {
                if (!tileResourcesDisplayed)
                    t.DisplayTileResources();
                else
                    t.HideTileResources();
            }
        }
        tileResourcesDisplayed = !tileResourcesDisplayed;
    }

    public void AddTileImprovement(Phase1TileImprovement improvement)
    {
        GridManager.instance.allTileImprovements.Add(improvement);
        // TODO: Change yield of affected tiles
    }

    public void RemoveTileImprovement(Phase1TileImprovement improvement)
    {
        throw new NotImplementedException();
    }
    
    void OnEnable()
    {
        EventManager.StartListening("NextRound", NextRound);
    }

    void OnDisable()
    {
        EventManager.StopListening("NextRound", NextRound);
    }

    void NextRound()
    {
        LocalPlayer.NextRound();
    }
}
