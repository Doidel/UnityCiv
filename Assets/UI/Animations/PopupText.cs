using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PopupText : MonoBehaviour {

    public Animator TextAnimator;
    private Text popupText;
    
	void Awake () {
        var clipInfo = TextAnimator.GetCurrentAnimatorClipInfo(0)[0];
        Destroy(gameObject, clipInfo.clip.length);

        popupText = TextAnimator.gameObject.GetComponent<Text>();
	}

    public void SetText(string text)
    {
        popupText.text = text;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
