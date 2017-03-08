using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {

    public Text Text;
    public Vector2 TooltipMousePos = new Vector2(10f, -10f);

	public void SetContent(string content)
    {
        Text.text = content;
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(Input.mousePosition.x + TooltipMousePos.x, Input.mousePosition.y + TooltipMousePos.y, 0);
    }

    public void SetAlignment(TooltipAlignment alignment)
    {
        GetComponent<RectTransform>().pivot = new Vector2(alignment == TooltipAlignment.Right ? 0 : 1, 1);
        Text.alignment = alignment == TooltipAlignment.Right ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
    }

    public enum TooltipAlignment
    {
        Right,
        Left
    }
}
