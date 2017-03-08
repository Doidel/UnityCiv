using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System;

public class TileValueDisplay : MonoBehaviour {

    public Image StrategicResourceImage;
    public GameObject StrategicResourceDisplay;
    public Image PopulatedDisplay;
    public RectTransform BasicResourceDisplay;
    public Image FoodIconPrefab;
    public Image ProductionIconPrefab;
    public int DistanceBetweenBasicResources;
    public int DistanceBetweenFoods;
    public int DistanceBetweenProductions;

    void Start()
    {
        gameObject.GetComponent<RectTransform>().localScale = new Vector3(0.01f, 0.01f, 0);
    }

    private List<Image> usedElements = new List<Image>();

    public void SetValues(Dictionary<Resource, int> resources, StrategicResource strategicResource, bool isUsed)
    {
        foreach (var e in usedElements)
        {
            Destroy(e.gameObject);
        }
        usedElements = new List<Image>();

        // if there's a strategic resource, display its image
        if (strategicResource == null)
        {
            StrategicResourceDisplay.SetActive(false);
        }
        else
        {
            StrategicResourceDisplay.SetActive(true);
            StrategicResourceImage.sprite = strategicResource.Icon;
        }
        
        PopulatedDisplay.color = new Color(1, 1, 1, isUsed ? 1 : 0);

        var food = resources.FirstOrDefault(r => r.Key is Food);
        var production = resources.FirstOrDefault(r => r.Key is Production);
        int totalSize = Math.Max(0, food.Value - 1) * DistanceBetweenFoods + Math.Min(1, food.Value * production.Value) * DistanceBetweenBasicResources + Math.Max(0, production.Value - 1) * DistanceBetweenProductions;
        int currentPos = (0 - totalSize) / 2;
        //int currentPos = 0;
        for (int f = 0; f < food.Value; f++)
        {
            if (f > 0)
                currentPos += DistanceBetweenFoods;
            Image ficon = Instantiate(FoodIconPrefab);
            ficon.transform.position = new Vector3(currentPos, 0, 0);
            ficon.transform.SetParent(BasicResourceDisplay, false);
            usedElements.Add(ficon);
        }

        currentPos += Math.Min(1, food.Value * production.Value) * DistanceBetweenBasicResources;

        for (int p = 0; p < production.Value; p++)
        {
            if (p > 0)
                currentPos += DistanceBetweenProductions;
            Image picon = Instantiate(ProductionIconPrefab);
            picon.transform.position = new Vector3(currentPos, 0, 0);
            picon.transform.SetParent(BasicResourceDisplay, false);
            usedElements.Add(picon);
        }
    }
}
