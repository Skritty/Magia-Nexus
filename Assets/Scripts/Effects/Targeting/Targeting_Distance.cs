using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Distance : MultiTargeting
{
    protected override int SortTargets(Entity e1, Entity e2)
    {
        int comparison = 0;
        float e1dist = Vector3.Distance(owner.transform.position, e1.transform.position);
        float e2dist = Vector3.Distance(owner.transform.position, e1.transform.position);
        if (sortingMethod == TargetSorting.Random) comparison = Random.Range(-1, 2);
        else if (e1dist > e2dist) comparison = 1;
        else if (e1dist < e2dist) comparison = -1;
        if (sortingMethod == TargetSorting.Highest) comparison *= -1;
        return comparison;
    }
}