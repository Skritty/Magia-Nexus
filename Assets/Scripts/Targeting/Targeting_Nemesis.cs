using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Nemesis : MultiTargeting
{
    public override List<Entity> GetTargets(Entity owner, Entity proxy = null)
    {
        List<Entity> targets = new List<Entity>();
        foreach (KeyValuePair<Viewer, float> player in owner.GetMechanic<Mechanic_PlayerOwner>().player.killedBy)
        {
            if (!(player.Key.character == null || !player.Key.character.gameObject.activeSelf))
                targets.Add(player.Key.character);
        }
        if (targets.Count == 0)
        {
            return base.GetTargets(owner);
        }

        if (sortingMethod != TargetSorting.Unsorted)
            targets.Sort((x, y) => SortTargets(owner, x, y));

        if (numberOfTargets >= 0)
        {
            int actualNumberOfTargets = numberOfTargets + (int)owner.Stat<Stat_AdditionalTargets>().Value;
            if (targets.Count > actualNumberOfTargets)
                targets.RemoveRange(actualNumberOfTargets, targets.Count - actualNumberOfTargets);
        }

        return targets;
    }

    protected override int SortTargets(Entity owner, Entity e1, Entity e2)
    {
        int comparison = 0;
        float e1Assist = owner.GetMechanic<Mechanic_PlayerOwner>().player.killedBy[e1.GetMechanic<Mechanic_PlayerOwner>().player];
        float e2Assist = owner.GetMechanic<Mechanic_PlayerOwner>().player.killedBy[e2.GetMechanic<Mechanic_PlayerOwner>().player];
        switch (sortingMethod)
        {
            case TargetSorting.Highest:
                {
                    if (e1Assist > e2Assist) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Lowest:
                {
                    if (e1Assist < e2Assist) comparison = -1;
                    else comparison = 1;
                    break;
                }
            case TargetSorting.Unsorted:
                {
                    comparison = Random.Range(-1, 2);
                    break;
                }
        }
        return comparison;
    }
}
