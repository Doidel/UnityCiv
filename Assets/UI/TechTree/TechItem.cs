using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public class TechItem : MonoBehaviour {

    [HideInInspector]
    public ResearchItem ResearchItem;
    public Image Icon;
    public Text Name;
    public Text Turns;
    public Image Background;
    public RectTransform FeaturePanel;
    public GameObject TechFeaturePrefab;
    [HideInInspector]
    public TechFeature[] TechFeatures;
    [HideInInspector]
    public TechTree TechTreeInstance;
    [HideInInspector]
    public Dictionary<ResearchItem, List<GameObject>> ChildLines;

    public void SetResearchItem(ResearchItem item)
    {
        ResearchItem = item;
        Icon.sprite = item.Image;
        Name.text = item.Title;
        gameObject.name = item.Title;
        UpdateTurns();

        // add the tech features
        var tfs = new List<TechFeature>();
        if (item.Features != null)
        {
            for (int fi = 0; fi < item.Features.Count; fi++)
            {
                var feature = item.Features[fi];
                var prefab = Instantiate(TechFeaturePrefab);
                prefab.GetComponent<RectTransform>().localPosition = new Vector3(-70 + fi * 44, 0, 0);
                prefab.transform.SetParent(FeaturePanel.transform, false);
                var tf = prefab.GetComponent<TechFeature>();
                tf.Icon.sprite = feature.Image;
                tf.Tooltip = feature.Description;
                tf.Parent = this;
                tfs.Add(tf);
            }
        }
        TechFeatures = tfs.ToArray();
    }

    public void OnClick()
    {
        TechTreeInstance.SelectItem(this);
    }
    
    public void PointerEnterEvent(BaseEventData baseEvent)
    {
        if (TechTreeInstance != null)
            TechTreeInstance.ToggleHighlight(this);
    }

    public void PointerExitEvent(BaseEventData baseEvent)
    {
        if (TechTreeInstance != null)
            TechTreeInstance.ToggleHighlight();
    }

    public void UpdateTurns()
    {
        Turns.text = Math.Round(ResearchItem.ProductionCosts / GameManager.instance.Research.ResearchProduction).ToString();
    }
}