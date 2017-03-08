using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UnitNumbersDisplay : MonoBehaviour {

    public GameObject UnitNumberBlock;

    private List<GameObject> activeBlocks = new List<GameObject>();

    public void SetUnits(IEnumerable<IGameUnit> units)
    {
        int blockI = 0;
        if (units.Count() > 1)
        {
            var unitGroups = units.GroupBy(u => u.GetUnitTypeIdentifier()).Select(g => new { Unit = g.Key, Count = g.Count() }).ToArray();
            for (int gi = 0; gi < unitGroups.Count(); gi++)
            {
                var group = unitGroups[gi];
                for (int i = 0; i < group.Count; i++)
                {
                    GameObject block;
                    if (blockI < activeBlocks.Count)
                    {
                        block = activeBlocks[gi];
                        block.SetActive(true);
                    }
                    else
                    {
                        block = Instantiate(UnitNumberBlock);
                        block.transform.SetParent(gameObject.transform, false);
                        activeBlocks.Add(block);
                    }

                    block.transform.localPosition = new Vector3(gi * 12, i * 6, 0);

                    blockI++;
                }
            }
        }
        //remove unused blocks
        for (; blockI < activeBlocks.Count; blockI++)
        {
            activeBlocks[blockI].SetActive(false);
        }
    }
}
