using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class Outpost : Phase1TileImprovement
{
	public static int SightRadius = 4;
	
    void Awake()
    {
        Name = "Outpost";
        Description = "Build an outpost!";
        Icon = Resources.Load<Sprite>("Icons/btn_outpost");
    }

    public override short BuildDurationRounds
    {
        get { return 5; }
    }

    public override Tile Location
    {
        get;
        set;
    }
}
