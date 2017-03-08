using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResearchItem : Object {

    public string Title;
    public Sprite Image;
    public string Tooltip;
    public float ProductionCosts;
    // LeadsTo can be buildItem, 
    public List<ResearchFeature> Features;
    public List<ResearchItem> Children;
    public int X;
    public int Y;
    public bool Completed;

    public class ResearchFeature
    {
        public Sprite Image;
        public string Description;
        public System.Action LeadsTo;
    }

    public override bool Equals(System.Object obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        ResearchItem p = obj as ResearchItem;
        if ((System.Object)p == null)
        {
            return false;
        }

        // Return true if the fields match:
        return Title == p.Title;
    }

    public bool Equals(ResearchItem p)
    {
        // If parameter is null return false:
        if ((object)p == null)
        {
            return false;
        }

        // Return true if the fields match:
        return Title == p.Title;
    }

    public override int GetHashCode()
    {
        return X ^ Y;
    }

    //add this code to class ThreeDPoint as defined previously
    //
    public static bool operator ==(ResearchItem a, ResearchItem b)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        // Return true if the fields match:
        return a.Title == b.Title;
    }

    public static bool operator !=(ResearchItem a, ResearchItem b)
    {
        return !(a == b);
    }
}
