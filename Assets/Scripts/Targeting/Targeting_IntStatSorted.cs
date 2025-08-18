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
        float e1hp = (e1.Stat(stat) as IDataContainer<float>).Value;
        float e2hp = (e2.Stat(stat) as IDataContainer<float>).Value;
        float e1Enmity = e1.Stat<Stat_Enmity>().Value;
        float e2Enmity = e2.Stat<Stat_Enmity>().Value;
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

public class Targeting_StatSortedInt : Targeting_StatSorted<int>
{
    protected override int SortTargets(Entity owner, Entity e1, Entity e2)
    {
        int comparison = 0;
        float e1hp = (e1.Stat(stat) as IDataContainer<int>).Value;
        float e2hp = (e2.Stat(stat) as IDataContainer<int>).Value;
        float e1Enmity = e1.Stat<Stat_Enmity>().Value;
        float e2Enmity = e2.Stat<Stat_Enmity>().Value;
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