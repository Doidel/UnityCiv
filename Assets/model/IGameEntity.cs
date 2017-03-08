using UnityEngine;
using System.Collections;

/// <summary>
/// An abstract class and some properties which all units and buildings in the game share
/// </summary>
public abstract class IGameEntity : MonoBehaviour {

    public string Name;
    public string Description;
    public Player OwnedBy;
    public Sprite Icon;
}
