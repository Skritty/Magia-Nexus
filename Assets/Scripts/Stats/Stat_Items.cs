using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Items : GenericStat<Stat_Items>
{
    protected override void Initialize()
    {
        foreach (Item item in items)
        {
            item.OnGained(owner);
        }
    }

    public Action<Entity> OnItemTick;

    [FoldoutGroup("Items"), ListDrawerSettings(ShowFoldout = false)]
    public List<Item> items = new List<Item>();

    public void AddItem(Item item)
    {
        items.Add(item);
        item.OnGained(owner);
    }

    public override void Tick()
    {
        OnItemTick?.Invoke(owner);
    }
}