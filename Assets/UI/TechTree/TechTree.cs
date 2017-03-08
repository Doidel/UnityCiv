using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TechTree : MonoBehaviour {

    public Color LineHighlightColor;
    public Sprite NormalBackground;
    public Sprite FinishedBackground;
    public Sprite AvailableBackground;
    public Sprite SelectedBackground;
    public Tooltip Tooltip;
    public GameObject TechItemsContainer;
    public GameObject TechItemDisplayPrefab;
    public GameObject TechLineHorizontal;
    public GameObject TechLineVertical;
    public GameObject TechLineHorizontalUp;
    public GameObject TechLineHorizontalDown;
    public GameObject TechLineUpHorizontal;
    public GameObject TechLineDownHorizontal;

    private float gridY = -100;
    private float gridX = 340;
    private float startX = 200;
    private float startY = -100;

    private Dictionary<ResearchItem, TechItem> ItemDisplays;
    private TechItem SelectedItem;

    public static TechTree instance;

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start() {
        ItemDisplays = new Dictionary<ResearchItem, TechItem>();
        BuildTree(GameManager.instance.Research.ResearchItems);
        foreach (var completedResearch in GameManager.instance.Research.ResearchItems.Where(r => r.Completed))
            SetFinished(completedResearch);
        gameObject.SetActive(false);
        // relocate techitems
        foreach (var item in ItemDisplays.Values)
        {
            item.transform.SetSiblingIndex(TechItemsContainer.transform.childCount - 1);
        }
        // relocate tooltip
        Tooltip.transform.SetSiblingIndex(TechItemsContainer.transform.childCount - 1);
	}

    void Update()
    {
        if (gameObject.activeSelf && Input.GetKey(KeyCode.Escape))
            ToggleActive();
    }

    public void SetFinished(ResearchItem item)
    {
        ItemDisplays[item].Background.sprite = FinishedBackground;
        item.Completed = true;

        if (item.Children != null)
        {
            foreach (var child in item.Children)
            {
                // check for each tile whether it becomes available, i.e. whether all parents have been completed
                if (!GameManager.instance.Research.ResearchItems.Where(r => r.Children != null && r.Children.Contains(child)).Any(r => !r.Completed))
                {
                    ItemDisplays[child].Background.sprite = AvailableBackground;
                }
            }
        }
    }

    public void SelectItem(TechItem techItem)
    {
        if (techItem.Background.sprite == AvailableBackground)
        {
            GameManager.instance.Research.SetCurrentResearch(techItem.ResearchItem);

            if (SelectedItem != null && SelectedItem.Background.sprite == SelectedBackground)
            {
                SelectedItem.Background.sprite = AvailableBackground;
            }
            techItem.Background.sprite = SelectedBackground;
            SelectedItem = techItem;

        }
    }

    private List<GameObject>[] _previouslyHighlightedChildren;
    private List<GameObject>[] _previouslyHighlightedParents;
    
    /// <summary>
    /// Highlights an item and all its connections
    /// Only removes highlight if no techitem is given, instead of switching it
    /// </summary>
    public void ToggleHighlight(TechItem item = null)
    {
        // reset
        if (_previouslyHighlightedChildren != null)
        {
            foreach (var line in _previouslyHighlightedChildren)
                foreach (var linesegment in line)
                    linesegment.GetComponent<Image>().color = Color.white;
            _previouslyHighlightedChildren = null;
        }
        if (_previouslyHighlightedParents != null)
        {
            foreach (var line in _previouslyHighlightedParents)
                foreach (var linesegment in line)
                    linesegment.GetComponent<Image>().color = Color.white;
            _previouslyHighlightedParents = null;
        }

        if (item != null)
        {
            if (item.ChildLines != null)
            {
                var children = item.ChildLines.Values.ToArray();
                foreach (var line in children)
                    foreach (var linesegment in line)
                    {
                        linesegment.GetComponent<Image>().color = LineHighlightColor;
                        linesegment.transform.SetSiblingIndex(TechItemsContainer.transform.childCount - 2 - ItemDisplays.Count);
                    }
                _previouslyHighlightedChildren = children;
            }

            var parents = ItemDisplays.SelectMany(i => i.Value.ChildLines).Where(cl => cl.Key == item.ResearchItem).Select(v => v.Value).ToArray();
            foreach (var line in parents)
                foreach (var linesegment in line)
                {
                    linesegment.GetComponent<Image>().color = LineHighlightColor;
                    linesegment.transform.SetSiblingIndex(TechItemsContainer.transform.childCount - 2 - ItemDisplays.Count);
                }
            _previouslyHighlightedParents = parents;

            item.transform.SetSiblingIndex(TechItemsContainer.transform.childCount - 2);
        }
    }

    public void ToggleFeatureTooltip(TechFeature feature = null)
    {
        if (feature != null)
        {
            Tooltip.SetContent(feature.Tooltip);
            Tooltip.gameObject.SetActive(true);
        } else
        {
            Tooltip.gameObject.SetActive(false);
        }
    }

    public void ToggleActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void BuildTree(ResearchItem[] researchItems)
    {
        Debug.Log("Build Research Tree");

        bool[,] occupiedFields = new bool[50, 8];
        foreach (var item in researchItems)
            occupiedFields[item.X, item.Y] = true;

        foreach (var item in researchItems)
        {
            var displayItem = Instantiate(TechItemDisplayPrefab);
            displayItem.transform.SetParent(TechItemsContainer.transform);
            displayItem.transform.position = new Vector3(startX + item.X * gridX, startY + item.Y * gridY, 0);
            var ti = displayItem.GetComponent<TechItem>();
            ti.SetResearchItem(item);
            ti.TechTreeInstance = this;
            ti.ChildLines = new Dictionary<global::ResearchItem, List<GameObject>>();
            ItemDisplays.Add(item, ti);
            // create connections to children
            if (item.Children != null)
                foreach (var child in item.Children) {
                    int distX = item.X - child.X - 1;
                    var line = CreateLine(item.X, item.Y, child.X, child.Y, distX == 0 ? true : Enumerable.Range(item.X + 1, child.X - 1).All(x => !occupiedFields[x, item.Y]));
                    ti.ChildLines.Add(child, line);
                }
        }
    }

    /// <summary>
    /// Creates a UI connection between a parent and a child position
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    private List<GameObject> CreateLine(int x1, int y1, int x2, int y2, bool horizontalFirst)
    {
        List<GameObject> createdTechLines = new List<GameObject>();
        float distanceX = x2 - x1 - 1;
        var connectionHorizontal1 = Instantiate(TechLineHorizontal);
        createdTechLines.Add(connectionHorizontal1);
        var transformHorizontal1 = connectionHorizontal1.GetComponent<RectTransform>();
        transformHorizontal1.SetParent(TechItemsContainer.transform);
        transformHorizontal1.position = new Vector3(startX + x1 * gridX + 137, startY + y1 * gridY, 0);

        // case: just horizontal
        if (y2 - y1 == 0)
        {
            transformHorizontal1.localScale = new Vector3(66 + distanceX * gridX, 1, 1);
        }
        else
        {
            float distanceX1 = horizontalFirst ? distanceX : 0;
            float distanceX2 = horizontalFirst ? 0 : distanceX;
            transformHorizontal1.localScale = new Vector3(23 + distanceX1 * gridX, 1, 1);
            float distance2 = Mathf.Abs(y2 - y1);
            float verticalPosX = transformHorizontal1.position.x + transformHorizontal1.localScale.x + 18;
            float verticalPosY = 0;

            // create a curve towards up or down
            if (y2 - y1 > 0)
            {
                var connectionToDown = Instantiate(TechLineHorizontalDown);
                createdTechLines.Add(connectionToDown);
                var transformToDown = connectionToDown.GetComponent<RectTransform>();
                transformToDown.SetParent(TechItemsContainer.transform);
                transformToDown.position = new Vector3(transformHorizontal1.position.x + transformHorizontal1.localScale.x, transformHorizontal1.position.y + 2);
                // create vertical path
                var connectionVertical = Instantiate(TechLineVertical);
                createdTechLines.Add(connectionVertical);
                var transformVertical = connectionVertical.GetComponent<RectTransform>();
                transformVertical.SetParent(TechItemsContainer.transform);
                verticalPosY = transformHorizontal1.position.y - 18;
                transformVertical.position = new Vector3(verticalPosX, verticalPosY);
                transformVertical.localScale = new Vector3(1, distance2 * -gridY - 2*18, 1);
                verticalPosY -= distance2 * -gridY - 2 * 18;
                // create second curve
                var connectionToHorizontal = Instantiate(TechLineDownHorizontal);
                createdTechLines.Add(connectionToHorizontal);
                var transformToHorizontal = connectionToHorizontal.GetComponent<RectTransform>();
                transformToHorizontal.SetParent(TechItemsContainer.transform);
                transformToHorizontal.position = new Vector3(verticalPosX - 2, verticalPosY);
                // create second horizontal part
                verticalPosX += 18;
                verticalPosY -= 18;
            }
            else
            {
                var connectionToUp = Instantiate(TechLineHorizontalUp);
                createdTechLines.Add(connectionToUp);
                var transformToUp = connectionToUp.GetComponent<RectTransform>();
                transformToUp.SetParent(TechItemsContainer.transform);
                transformToUp.position = new Vector3(transformHorizontal1.position.x + transformHorizontal1.localScale.x, transformHorizontal1.position.y + 18);
                // create vertical path
                var connectionVertical = Instantiate(TechLineVertical);
                createdTechLines.Add(connectionVertical);
                var transformVertical = connectionVertical.GetComponent<RectTransform>();
                transformVertical.SetParent(TechItemsContainer.transform);
                float verticalSizeY = distance2 * -gridY - 2 * 18;
                verticalPosY = transformHorizontal1.position.y + 18 + verticalSizeY;
                transformVertical.position = new Vector3(verticalPosX, verticalPosY);
                transformVertical.localScale = new Vector3(1, verticalSizeY, 1);
                // create second curve
                var connectionToHorizontal = Instantiate(TechLineUpHorizontal);
                createdTechLines.Add(connectionToHorizontal);
                var transformToHorizontal = connectionToHorizontal.GetComponent<RectTransform>();
                transformToHorizontal.SetParent(TechItemsContainer.transform);
                verticalPosY += 20;
                transformToHorizontal.position = new Vector3(verticalPosX - 2, verticalPosY);
                // create second horizontal part
                verticalPosX += 18;
                verticalPosY -= 2;
            }
            var connectionHorizontal2 = Instantiate(TechLineHorizontal);
            createdTechLines.Add(connectionHorizontal2);
            var transformHorizontal2 = connectionHorizontal2.GetComponent<RectTransform>();
            transformHorizontal2.SetParent(TechItemsContainer.transform);
            transformHorizontal2.position = new Vector3(verticalPosX, verticalPosY, 0);
            transformHorizontal2.localScale = new Vector3(23 + distanceX2 * gridX, 1, 1);
        }

        return createdTechLines;
    }
}
