using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BuildOrder
{
    public BuildItem Item;
    public float Produced; // amount of production units flown into this order

    public BuildOrder(BuildItem item, float produced)
    {
        Item = item;
        Produced = produced;
    }
}
