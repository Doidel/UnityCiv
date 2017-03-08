using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TechFeature : MonoBehaviour {

    public string Tooltip;
    public Image Icon;
    [HideInInspector]
    public TechItem Parent;

    public void PointerEnterEvent(BaseEventData baseEvent)
    {
        if (Parent.TechTreeInstance != null)
            Parent.TechTreeInstance.ToggleFeatureTooltip(this);
    }

    public void PointerExitEvent(BaseEventData baseEvent)
    {
        if (Parent.TechTreeInstance != null)
            Parent.TechTreeInstance.ToggleFeatureTooltip();
    }
}
