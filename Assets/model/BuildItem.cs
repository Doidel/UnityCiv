using UnityEngine;
using System.Collections;

[System.Serializable]
public class BuildItem {
    public string Title;
	private Sprite imageSprite;
    public Sprite Image {
		get {
			if (imageSprite == null && !string.IsNullOrEmpty(ImageAssetPath))
				imageSprite = Resources.Load<Sprite>(ImageAssetPath);
			return imageSprite;
		}
	}
    public string ImageAssetPath;
    public string Tooltip;
    public float ProductionCosts;
    public float PurchaseCosts;
    public string ProducesAssetPath;
	private GameObject producesGO;
    public GameObject Produces {
		get {
            if (producesGO == null && !string.IsNullOrEmpty(ProducesAssetPath))
            {
                producesGO = Resources.Load<GameObject>(ProducesAssetPath);
            }
			return producesGO;
		}
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="image"></param>
    /// <param name="tooltip"></param>
    /// <param name="productionCosts"></param>
    /// <param name="purchaseCosts">Amount of gold the item can be purchased for. 0 = item can't be purchased.</param>
    public BuildItem(string title, string imageAssetPath, string tooltip, float productionCosts, float purchaseCosts, string producesAssetPath)
    {
        Title = title;
        ImageAssetPath = imageAssetPath;
        Tooltip = tooltip;
        ProductionCosts = productionCosts;
        PurchaseCosts = purchaseCosts;
        ProducesAssetPath = producesAssetPath;
    }
}
