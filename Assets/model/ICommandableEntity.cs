using UnityEngine;
using System.Collections;

public abstract class AbstractCommandableEntity : IGameEntity
{
    
    public Canvas UnitCanvas { get; set; }

    public AudioSource audioSource { get; protected set; }

    protected virtual void Awake()
    {
        OwnedBy = GameManager.instance.LocalPlayer;
        InitUnitCanvas();
        InitAudioSource();
        TimeManager.instance.NeedNewOrders(this);
    }

    protected void InitUnitCanvas()
    {
        UnitCanvas = Instantiate(Resources.Load<Canvas>("Prefabs/UnitCanvas"));
        UnitCanvas.transform.SetParent(transform, false);
    }

    protected void InitAudioSource()
    {
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.maxDistance = 20;
        audioSource.minDistance = 2;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    public abstract void Select();

    /// <summary>
    /// Returns whether the unit still needs orders for this turn or whether it knows what to do
    /// </summary>
    /// <returns></returns>
    public abstract bool NeedsOrders();
}
