using Unity.VisualScripting;
using UnityEngine;

public class Targeting_StatSorted<T> : MultiTargeting
{
    [SerializeReference]
    public IStat<T> stat;
}

public class Targeting_StatSortedFloat : Targeting_StatSorted<float>
{
    protected override int SortTargets(Entity owner, Entity e1, Entity e2)
    {
        int comparison = 0;
        float e1Stat = (e1.GetStat(stat) as IValueContainer<float>).Value;
        float e2Stat = (e2.GetStat(stat) as IValueContainer<float>).Value;
        float e1Enmity = Stats.GetStat<Stat_Enmity>(e1).Value;
        float e2Enmity = Stats.GetStat<Stat_Enmity>(e2).Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1Stat * e1Enmity > e2Stat * e2Enmity) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Lowest:
                {
                    if (e1Stat / e1Enmity < e2Stat / e2Enmity) comparison = -1;
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

public class Targeting_StatSortedFloatPercent : Targeting_StatSorted<float>
{
    [SerializeReference]
    public IStat<float> maximum;
    protected override int SortTargets(Entity owner, Entity e1, Entity e2)
    {
        int comparison = 0;
        float e1Stat = (e1.GetStat(stat) as IValueContainer<float>).Value / (e1.GetStat(maximum) as IValueContainer<float>).Value;
        float e2Stat = (e2.GetStat(stat) as IValueContainer<float>).Value / (e2.GetStat(maximum) as IValueContainer<float>).Value;
        float e1Enmity = Stats.GetStat<Stat_Enmity>(e1).Value;
        float e2Enmity = Stats.GetStat<Stat_Enmity>(e2).Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1Stat * e1Enmity > e2Stat * e2Enmity) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Lowest:
                {
                    if (e1Stat / e1Enmity < e2Stat / e2Enmity) comparison = -1;
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

public class Targeting_StatSortedInt : Targeting_StatSorted<int>
{
    protected override int SortTargets(Entity owner, Entity e1, Entity e2)
    {
        int comparison = 0;
        int e1Stat = (e1.GetStat(stat) as IValueContainer<int>).Value;
        int e2Stat = (e2.GetStat(stat) as IValueContainer<int>).Value;
        float e1Enmity = Stats.GetStat<Stat_Enmity>(e1).Value;
        float e2Enmity = Stats.GetStat<Stat_Enmity>(e2).Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1Stat * e1Enmity > e2Stat * e2Enmity) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Lowest:
                {
                    if (e1Stat / e1Enmity < e2Stat / e2Enmity) comparison = -1;
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

public class Targeting_StatSortedIntPercent : Targeting_StatSorted<int>
{
    [SerializeReference]
    public IStat<int> maximum;
    protected override int SortTargets(Entity owner, Entity e1, Entity e2)
    {
        int comparison = 0;
        int e1Stat = (e1.GetStat(stat) as IValueContainer<int>).Value / (e1.GetStat(maximum) as IValueContainer<int>).Value;
        int e2Stat = (e2.GetStat(stat) as IValueContainer<int>).Value / (e2.GetStat(maximum) as IValueContainer<int>).Value;
        float e1Enmity = Stats.GetStat<Stat_Enmity>(e1).Value;
        float e2Enmity = Stats.GetStat<Stat_Enmity>(e2).Value;
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1Stat * e1Enmity > e2Stat * e2Enmity) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Lowest:
                {
                    if (e1Stat / e1Enmity < e2Stat / e2Enmity) comparison = -1;
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