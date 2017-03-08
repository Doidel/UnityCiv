using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Healthbar : MonoBehaviour
{
    public RectTransform healthBarInner;
    //private CivCamera camera;

    public void Set(int health, int maxhealth)
    {
        gameObject.SetActive(health != maxhealth);
        healthBarInner.sizeDelta = new Vector2(healthBarInner.sizeDelta.x, 40f / maxhealth * health);
    }

    void Start()
    {
        gameObject.SetActive(false);
    }
}