using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TopPanel : MonoBehaviour {

    public Image PopulationProgressbar;
    public Text PopulationText;

    public void UpdatePopulation(int populationCount)
    {
        PopulationText.text = populationCount.ToString();
    }

    public void UpdatePopulationProgress(double progress)
    {
        PopulationProgressbar.GetComponent<RectTransform>().sizeDelta = new Vector2(60f * (float)progress, 4);
    }

    public static TopPanel instance = null;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
