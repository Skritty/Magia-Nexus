using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Distance : MultiTargeting
{
    protected override int SortTargets(Entity owner, Entity e1, Entity e2)
    {
        int comparison = 0;
        float e1dist = Vector3.Distance(owner.transform.position, e1.transform.position);
        float e2dist = Vector3.Distance(owner.transform.position, e2.transform.position);
        float e1Enmity = e1.Stat<Stat_Enmity>().Value;
        float e2Enmity = e2.Stat<Stat_Enmity>().Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1dist * e1Enmity > e2dist * e2Enmity) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Lowest:
                {
                    if (e1dist / e1Enmity < e2dist / e2Enmity) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Unsorted:
                {
                    if (e1Enmity == e2Enmity) comparison = Random.Range(-1, 2);
                    else if (e1Enmity > e2Enmity) comparison = -1;
                    else comparison = 1;
                    break;
                }
        }
        return comparison;
    }
}

// O ---- T1 (DPS) -- T2 (Tank) dist = 4 vs 6 33% enmity to match
// T1 (DPS) = 100 life vs T2 (Tank) = 300 life 200% enmity to match lowest life
// Lowest: life / (1 + enmity) = 200% enmity
// Highest: life * (1 + enmity) = 200% enmity