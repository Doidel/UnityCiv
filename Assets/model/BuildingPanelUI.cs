using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingPanelUI : MonoBehaviour
{
    public static BuildingPanelUI instance = null;
    
    public RectTransform buildPanelContainer;
    public GameObject buildItemTemplate;
    public Text currentlyBuildingTitle;
    public Image currentlyBuildingIcon;
    public Image currentlyBuildingProgressImage;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start ()
    {
        //gameObject.SetActive(false);
    }

    public void SetBuildItems(BuildItem[] items, int ProductionOutput)
    {
        if (items != null)
        {
            foreach (Transform child in buildPanelContainer.transform)
            {
                Destroy(child.gameObject);
            }

            gameObject.SetActive(true);

            foreach (var item in items)
            {
                GameObject newItem = Instantiate<GameObject>(buildItemTemplate);
                BuildingItemPrefab newItem_vals = newItem.GetComponent<BuildingItemPrefab>();
                newItem_vals.title.text = item.Title;
                newItem_vals.icon.sprite = item.Image;
                newItem_vals.turns.text = ((int)item.ProductionCosts / ProductionOutput) + " Turns";
                var li = item;
                newItem_vals.button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => SelectItem(li)));
                newItem.transform.SetParent(buildPanelContainer);
            }

            SetCurrentlyBuilding();
        }
        else
        {
            gameObject.SetActive(false);
            GridManager.instance.HideAreaOfControl();
        }
    }

    public void SelectItem(BuildItem item)
    {
        if (GridManager.instance.selectedBuilding != null)
        {
            GameManager.instance.PlayUIClick();
            ProductionGameBuilding building = GridManager.instance.selectedBuilding.GetComponent<ProductionGameBuilding>();
            building.Produce(item);
            SetCurrentlyBuilding();
        }
    }

    /// <summary>
    /// Update the currently-producing-display with the selected building
    /// </summary>
    public void SetCurrentlyBuilding()
    {
        if (GridManager.instance.selectedBuilding != null)
        {
            ProductionGameBuilding building = GridManager.instance.selectedBuilding.GetComponent<ProductionGameBuilding>();
            currentlyBuildingTitle.gameObject.SetActive(building.Producing != null);
            currentlyBuildingIcon.gameObject.SetActive(building.Producing != null);

            if (building.Producing != null)
            {
                currentlyBuildingIcon.sprite = building.Producing.Item.Image;
                currentlyBuildingTitle.text = building.Producing.Item.Title.ToUpper() + " (" + ((building.Producing.Item.ProductionCosts - building.Producing.Produced) / building.ProductionOutput) + "Turns )";
                currentlyBuildingProgressImage.fillAmount = building.Producing.Produced / building.Producing.Item.ProductionCosts;
            }
            else
            {
                currentlyBuildingProgressImage.fillAmount = 0;
            }
        }
    }
}
