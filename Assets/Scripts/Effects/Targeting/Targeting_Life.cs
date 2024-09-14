using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Life : MultiTargeting
{
    protected override int SortTargets(Entity e1, Entity e2)
    {
        int comparison = 0;
        float e1hp = e1.Stat<Stat_Life>().currentLife;
        float e2hp = e2.Stat<Stat_Life>().currentLife;
        if (sortingMethod == TargetSorting.Random) comparison = Random.Range(-1, 2);
        else if (e1hp > e2hp) comparison = 1;
        else if (e1hp < e2hp) comparison = -1;
        if (sortingMethod == TargetSorting.Highest) comparison *= -1;
        return comparison;
    }
}