using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public class UnitPanelUI : MonoBehaviour
{
    public UnitActionButton UnitActionButtonPrefab;
    
    public static UnitPanelUI instance = null;
    private List<UnitActionButton> Buttons = new List<UnitActionButton>();

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
        for (int i = 0; i < 7; i++)
        {
            var go = Instantiate(UnitActionButtonPrefab);
            go.transform.position = new Vector3(10 + i * 60, 0, 0);
            go.transform.SetParent(transform, false);
            Buttons.Add(go);
        }
    }

    public void SetUnit(IGameUnit unit)
    {
        if (unit != null)
        {
            gameObject.SetActive(true);
            for (int i = 0; i < Buttons.Count; i++)
            {
                if (i < unit.Actions.Count)
                {
                    // the button will be used
                    var button = Buttons[i];
                    button.gameObject.SetActive(true);
                    button.UnitAction = unit.Actions[i];

                    if (unit.Producing.HasValue && unit.Producing.Value.Name == unit.Actions[i].Name)
                    {
                        if (unit.Producing.Value.Action != null && unit.ProducingRoundsLeft < 1000)
                        {
                            var unitActionGO = (GameObject)unit.Producing.Value.Action;
                            button.SetFillAmount(1 - unit.ProducingRoundsLeft / unitActionGO.GetComponent<Phase1TileImprovement>().BuildDurationRounds);
                        }
                        else
                        {
                            // either Action is no GO or RoundsLeft is huge
                            button.SetFillAmount(0);
                            button.SetActionActive(true);
                        }
                    }
                    else
                    {
                        button.SetFillAmount(0);
                        button.SetActionActive(false);
                    }
                }
                else
                {
                    Buttons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
