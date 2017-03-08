using UnityEngine;
using System.Collections;

public abstract class ProductionGameBuilding : AbstractCommandableEntity, IGameBuilding
{

    public int AreaOfControl = 4;

    public abstract BuildOrder Producing
    {
        get;
        protected set;
    }

    public abstract float ProductionOutput { get; protected set; }

    public abstract BuildItem[] Items
    {
        get;
        protected set;
    }
    public abstract string Tooltip { get; }
    public abstract int BuildDurationRounds { get; }
    public abstract Tile Location { get; set; }

    public override void Select()
    {
        GridManager.instance.selectedBuilding = gameObject;
        audioSource.PlayOneShot(GameManager.instance.Select1, 0.2f);
        UnitPanelUI.instance.SetUnit(null);
    }

    protected virtual void Start()
    {
        GridManager.instance.allBuildings.Add(gameObject);
    }

    /// <summary>
    /// Remove a building from the game with this method instead of directly calling "Destroy"
    /// </summary>
    public void RemoveBuilding()
    {
        Destroy(gameObject);
        if (GridManager.instance.selectedBuilding == gameObject)
        {
            GridManager.instance.selectedBuilding = null;
            BuildingPanelUI.instance.SetBuildItems(null, 0);
        }
        GridManager.instance.allBuildings.Remove(gameObject);
    }

    public abstract void Produce(BuildItem item);
}
