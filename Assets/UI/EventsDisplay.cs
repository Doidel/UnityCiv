using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class EventsDisplay : MonoBehaviour {

    public EventDisplayItem EventDisplayPrefab;
    public Tooltip EventTooltip;
    private Transform MainCanvas;

    public List<EventDisplayItem> Items = new List<EventDisplayItem>();

    public static EventsDisplay instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        MainCanvas = GameObject.Find("MainCanvas").transform;
        EventTooltip.SetAlignment(Tooltip.TooltipAlignment.Left);
    }

    void Update () {
        // set all items which haven't reached their destination one step further
        var movementDistance = 500 * Time.deltaTime;
        foreach (var el in Items)
        {
            var distance = el.transform.position.y - el.TargetPos.y;
            if (distance > 0)
            {
                if (distance > movementDistance)
                    el.transform.position = new Vector3(el.transform.position.x, el.transform.position.y - movementDistance);
                else
                    el.transform.position = el.TargetPos;
            }
        }
	}

    public void AddItem(Sprite image, string tooltip, Action clickaction = null)
    {
        var newEvent = Instantiate(EventDisplayPrefab);
        newEvent.Image.sprite = image;
        newEvent.TooltipText = tooltip;
        newEvent.transform.SetParent(MainCanvas, false);
        // set initial start pos. Target pos will be set by repositionItems
        newEvent.transform.position = new Vector3(newEvent.transform.position.x, 180 + 120 * Items.Count + 300);
        Items.Add(newEvent);
        RepositionItems();
    }

    public void RepositionItems()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            var newTargetPos = new Vector3(Items[i].transform.position.x, 180 + 65 * i);
            if (Items[i].DestinationReached()) {
                Items[i].transform.position = newTargetPos;
            }
            Items[i].TargetPos = newTargetPos;
        }
    }

    public void ClearEvents()
    {
        foreach (var item in Items)
            Destroy(item.gameObject);
        Items.Clear();
    }
}
