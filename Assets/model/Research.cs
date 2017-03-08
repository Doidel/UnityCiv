using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Research {

    // X: 0 - 6, Y: 0 - ?
    public readonly ResearchItem[] ResearchItems = new ResearchItem[]
    {
        new ResearchItem()
        {
            Title = "From Nomads to Farmers",
            Tooltip = "When Nomads became Farmers...",
            ProductionCosts = 0,
            X = 0,
            Y = 3,
            Completed = true,
            Image = Resources.Load<Sprite>("Icons/from_nomads_to_farmers")
        },
        new ResearchItem()
        {
            Title = "Natural Religion",
            Tooltip = "Introduces the idea of divinity",
            ProductionCosts = 300,
            X = 1,
            Y = 6,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
			Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Sacrificial Offering. Allows a worker to burn 1 food and gain 1 production in the next turn."
				},
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Blessed land. The village can declare a piece of land as blessed, increasing population growth by 5%."
				}
			}
        },
        new ResearchItem()
        {
            Title = "Fishing",
            Tooltip = "Research Fishing",
            ProductionCosts = 3,
            X = 1,
            Y = 4,
            Image = Resources.Load<Sprite>("Icons/fishing"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Improves the food yield of fish by 1 for each tile",
                    LeadsTo = () => ResearchEffects.ChangeYield("Fish", new Dictionary<Resource, int>()
                        {
                            { Food.i, 1 },
                            { Production.i, 0 }
                        })
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Fishing hut",
                    LeadsTo = () => ResearchEffects.UnlockBuilding(GameManager.AllBuildItems.First(a => a.Title == "Fishing Hut"))
                }
            }
        },
        new ResearchItem()
        {
            Title = "Stone Tools",
            Tooltip = "Research Stone Tools",
            ProductionCosts = 300,
            X = 1,
            Y = 3,
            Image = Resources.Load<Sprite>("Icons/stone_tools"),
			Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Basic tools. Increases village production by 1."
				}
			}
        },
        new ResearchItem()
        {
            Title = "Hunting",
            Tooltip = "Research Stone Tools",
            ProductionCosts = 3,
            X = 1,
            Y = 0,
            Image = Resources.Load<Sprite>("Icons/stone_tools"),
			Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/hunting_shack"),
                    Description = "Hunting shack. Increases food yield from surrounding wildlife by 1.",
                    LeadsTo = () => ResearchEffects.UnlockBuilding(GameManager.AllBuildItems.First(a => a.Title == "Hunting Shack"))
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/icon_worker"),
                    Description = "Hunter",
                    LeadsTo = () => ResearchEffects.UnlockUnit(GameManager.AllBuildItems.First(a => a.Title == "Hunter"))
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Workers can build Outposts",
                    LeadsTo = () => ResearchEffects.UnlockUnitAction("BuildOutpost")

                }
            }
        },
        new ResearchItem()
        {
            Title = "Burial Rites",
            Tooltip = "Research Burial Rites",
            ProductionCosts = 300,
            X = 2,
            Y = 6,
            Image = Resources.Load<Sprite>("Icons/stone_tools"),
			Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Burial ceremony. Increases population growth by 5%."
				},
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Burial grounds"
				}
			}
        },
        new ResearchItem()
        {
            Title = "Wood Working",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 2,
            Y = 4,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
			Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Lumberjack"
                }
            }
        },
        new ResearchItem()
        {
            Title = "Mining",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 2,
            Y = 3,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
			Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Rocks mining. Mining of flint, sandstone, basalt, chert and other rocks increase the village's productivity by 1."
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Improved hunting weaponry. Increases the hunter's attack by 1."
                }
            }
        },
        new ResearchItem()
        {
            Title = "Farming",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 2,
            Y = 1,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
			Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Introduces basic agricultural ideas. Enables creating a field of lentils."
				},
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Farm. Allows crops to be seed in its neighbouring tiles."
				},
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/icon_worker"),
                    Description = "Farmer"
				}
			}
        },
        new ResearchItem()
        {
            Title = "Animal Husbandry",
            Tooltip = "Research Animal Husbandry",
            ProductionCosts = 300,
            X = 2,
            Y = 0,
            Image = Resources.Load<Sprite>("Icons/animal_husbandry"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Animal flocks. Enables workers to gather domesticable animals."
				}
            }
        },
        new ResearchItem()
        {
            Title = "Places of Worship",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 3,
            Y = 6,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/religion"),
                    Description = "Introduces a basic idea of religion."
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Shrine"
                }
            }
        },
        new ResearchItem()
        {
            Title = "Writing",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 3,
            Y = 5,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Alphabet. Increases the aspect ratio of population to science gain by 4%."
                }
            }
        },
        new ResearchItem()
        {
            Title = "Stonecutting",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 3,
            Y = 3,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Quarry"
                }
            }
        },
        new ResearchItem()
        {
            Title = "Clay and Pottery",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 3,
            Y = 2,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "?"
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "?"
                }
            }
        },
        new ResearchItem()
        {
            Title = "Animal Breeding",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 3,
            Y = 0,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Enables Farmers to rais animals"
                }
            }
        },
        new ResearchItem()
        {
            Title = "Shamanism",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 4,
            Y = 7,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/icon_worker"),
                    Description = "Shaman"
                }
            }
        },
        new ResearchItem()
        {
            Title = "Basic Masonry",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 4,
            Y = 4,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "More stable and water-proof housing leads to 5% higher population growth."
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Hut. Housing for your villagers, increasing maximum population by 10 each."
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Village center upgrade"
                }
            }
        },
        new ResearchItem()
        {
            Title = "Food Preservation",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 4,
            Y = 3,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Food Storage Cellar. +2 food in the village center. Needs no tile. Either a Granary or a Food Storage can be built."
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Granary. +3 food on the tile it is built upon. Either a Granary or a Food Storage can be built."
                }
            }
        },
        new ResearchItem()
        {
            Title = "Art",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 4,
            Y = 2,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Maybe introduces happiness?"
                }
            }
        },
        new ResearchItem()
        {
            Title = "Plow",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 4,
            Y = 0,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Wheat field"
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Enables Farmer to plow, increasing the tile's food yield by 1 for 5 turns."
                }
            }
        },
        new ResearchItem()
        {
            Title = "Temples",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 5,
            Y = 6,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Temple. +3 belief on the tile it's built upon."
                }
            }
        },
        new ResearchItem()
        {
            Title = "Solar Calendar",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 5,
            Y = 5,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "?"
                }
            }
        },
        new ResearchItem()
        {
            Title = "Wells",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 5,
            Y = 4,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/fishinghut"),
                    Description = "Well. Buildable in village center, requires no tile. +3 food."
                }
            }
        },
        new ResearchItem()
        {
            Title = "Jewelry",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 5,
            Y = 2,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Introcudes a first form of currency."
                },
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "Increases happiness by 1."
                }
            }
        },
        new ResearchItem()
        {
            Title = "The Wheel",
            Tooltip = "Research Wood Working",
            ProductionCosts = 300,
            X = 6,
            Y = 3,
            Image = Resources.Load<Sprite>("Icons/wood_working"),
            Features = new List<ResearchItem.ResearchFeature>()
            {
                new ResearchItem.ResearchFeature() {
                    Image = Resources.Load<Sprite>("Icons/research_yieldFeature"),
                    Description = "A new era dawns. Your settlement gains mobility and seeks to expand. Key technology to enter <color=#ffa500ff>Era II</color>."
                }
            }
        }
    };
    
    // How much Research does the player complete per round?
    public int ResearchProduction;

    private ResearchItem CurrentResearch;

    public void SetCurrentResearch(ResearchItem item)
    {
        CurrentResearch = item;
    }

    public Research()
    {
        // base research production
        ResearchProduction = 3;

        var ri = ResearchItems;
        Func<string, ResearchItem> bt = (string s) =>
        {
            return ri.First(r => r.Title == s);
        };

        // connect researches
        ri[0].Children = new List<ResearchItem>() { bt("Natural Religion"), bt("Fishing"), bt("Stone Tools"), bt("Hunting") };
        bt("Natural Religion").Children = new List<ResearchItem>() { bt("Burial Rites"), bt("Writing") };
        bt("Stone Tools").Children = new List<ResearchItem>() { bt("Wood Working"), bt("Mining"), bt("Farming") };
        bt("Hunting").Children = new List<ResearchItem>() { bt("Animal Husbandry") };
        bt("Burial Rites").Children = new List<ResearchItem>() { bt("Places of Worship") };
        bt("Wood Working").Children = new List<ResearchItem>() { bt("Basic Masonry") };
        bt("Mining").Children = new List<ResearchItem>() { bt("Stonecutting"), bt("Clay and Pottery") };
        bt("Farming").Children = new List<ResearchItem>() { bt("Plow") };
        bt("Animal Husbandry").Children = new List<ResearchItem>() { bt("Animal Breeding") };
        bt("Places of Worship").Children = new List<ResearchItem>() { bt("Shamanism"), bt("Temples") };
        bt("Writing").Children = new List<ResearchItem>() { bt("Temples"), bt("Solar Calendar") };
        bt("Stonecutting").Children = new List<ResearchItem>() { bt("Basic Masonry") };
        bt("Clay and Pottery").Children = new List<ResearchItem>() { bt("Food Preservation"), bt("Art") };
        bt("Animal Breeding").Children = new List<ResearchItem>() { bt("Plow") };
        bt("Basic Masonry").Children = new List<ResearchItem>() { bt("Temples"), bt("Solar Calendar"), bt("Wells"), bt("The Wheel") };
        bt("Art").Children = new List<ResearchItem>() { bt("Jewelry") };
        bt("Plow").Children = new List<ResearchItem>() { bt("The Wheel") };
    }

    public void Start()
    {
        // complete first research by artificially calling the next round listener
        CurrentResearch = ResearchItems.First();
        CurrentResearch.ProductionCosts += ResearchProduction;
    }

    // Called when the player puts his first village. Then research can begin.
    public void StartResearch()
    {
        NextRoundListener();
        EventManager.StartListening("NextRound", NextRoundListener);
    }

    public bool HasResearchSelected()
    {
        return CurrentResearch != null;
    }

    private void NextRoundListener()
    {
        Debug.Log("Next round research");
        CurrentResearch.ProductionCosts -= ResearchProduction;
        if (CurrentResearch.ProductionCosts <= 0)
        {
            CurrentResearch.Completed = true;
            TechTree.instance.SetFinished(CurrentResearch);
            if (CurrentResearch.Features != null)
            {
                foreach (var lt in CurrentResearch.Features)
                {
                    lt.LeadsTo();
                }
            }
            EventsDisplay.instance.AddItem(TimeManager.instance.NextRoundResearchSprite, CurrentResearch.Title + " researched!");
            CurrentResearch = null;
        }
    }
}
