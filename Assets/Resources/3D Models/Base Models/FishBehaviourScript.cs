using UnityEngine;
using System.Collections;

public class FishBehaviourScript : MonoBehaviour {

    public GameObject Fish1;
    public GameObject Fish2;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.Rotate(new Vector3(0f, -Time.deltaTime * 36f, 0f));
    }
}
