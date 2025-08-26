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
        float e1Stat = (e1.Stat(stat) as IDataContainer<float>).Value;
        float e2Stat = (e2.Stat(stat) as IDataContainer<float>).Value;
        float e1Enmity = e1.Stat<Stat_Enmity>().Value;
        float e2Enmity = e2.Stat<Stat_Enmity>().Value;
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
        float e1Stat = (e1.Stat(stat) as IDataContainer<float>).Value / (e1.Stat(maximum) as IDataContainer<float>).Value;
        float e2Stat = (e2.Stat(stat) as IDataContainer<float>).Value / (e2.Stat(maximum) as IDataContainer<float>).Value;
        float e1Enmity = e1.Stat<Stat_Enmity>().Value;
        float e2Enmity = e2.Stat<Stat_Enmity>().Value;
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
        int e1Stat = (e1.Stat(stat) as IDataContainer<int>).Value;
        int e2Stat = (e2.Stat(stat) as IDataContainer<int>).Value;
        float e1Enmity = e1.Stat<Stat_Enmity>().Value;
        float e2Enmity = e2.Stat<Stat_Enmity>().Value;
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
        int e1Stat = (e1.Stat(stat) as IDataContainer<int>).Value / (e1.Stat(maximum) as IDataContainer<int>).Value;
        int e2Stat = (e2.Stat(stat) as IDataContainer<int>).Value / (e2.Stat(maximum) as IDataContainer<int>).Value;
        float e1Enmity = e1.Stat<Stat_Enmity>().Value;
        float e2Enmity = e2.Stat<Stat_Enmity>().Value;
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