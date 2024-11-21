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
        float e1Enmity = e1.Stat<Stat_EffectModifiers>().CalculateModifier(EffectTag.Enmity);
        float e2Enmity = e2.Stat<Stat_EffectModifiers>().CalculateModifier(EffectTag.Enmity);
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1hp * e1Enmity > e2hp * e2Enmity) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Lowest:
                {
                    if (e1hp / e1Enmity < e2hp / e2Enmity) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Random:
                {
                    comparison = Random.Range(-1, 2);
                    break;
                }
        }
        return comparison;
    }
}