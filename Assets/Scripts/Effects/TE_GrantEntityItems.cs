using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TE_GrantEntityItems : Effect
{
    [SerializeReference]
    public List<Item> items = new();
    public override void Activate()
    {
        foreach (Item item in items)
        {
            target.Stat<Stat_Items>().AddItem(item);
        }
    }
}