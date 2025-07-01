using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_LifePercent : MultiTargeting
{
    protected override int SortTargets(Entity e1, Entity e2)
    {
        int comparison = 0;
        float e1hp = e1.Stat<Stat_CurrentLife>().Value / e2.Stat<Stat_MaxLife>().Value;
        float e2hp = e2.Stat<Stat_CurrentLife>().Value / e2.Stat<Stat_MaxLife>().Value;
        float e1Enmity = e1.GetMechanic<Stat_EffectModifiers>().CalculateModifier(EffectTag.Enmity);
        float e2Enmity = e2.GetMechanic<Stat_EffectModifiers>().CalculateModifier(EffectTag.Enmity);
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