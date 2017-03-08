using UnityEngine;
using System.Collections;

public abstract class Phase1TileImprovement : IGameEntity, IGameBuilding
{
    public abstract short BuildDurationRounds { get; }

    [HideInInspector]
    public abstract Tile Location { get; set; }

    /*public AudioSource audioSource { get; protected set; }
    protected virtual void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.maxDistance = 20;
        audioSource.minDistance = 2;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }*/
}
