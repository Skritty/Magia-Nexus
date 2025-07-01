using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting_Nemesis : MultiTargeting
{
    public override List<Entity> GetTargets(Effect source, Entity owner, Entity proxy = null)
    {
        lockTarget = true;
        if (lockTarget && !(primaryTarget == null || !primaryTarget.gameObject.activeSelf))
        {
            return targets;
        }
        targets = new List<Entity>();
        foreach (KeyValuePair<Viewer, float> player in owner.GetMechanic<Stat_PlayerOwner>().player.killedBy)
        {
            if (!(player.Key.character == null || !player.Key.character.gameObject.activeSelf))
                targets.Add(player.Key.character);
        }
        if (targets.Count == 0)
        {
            return base.GetTargets(source, owner);
        }

        if (sortingMethod != TargetSorting.Unsorted)
            targets.Sort(SortTargets);

        int actualNumberOfTargets = numberOfTargets + owner.GetMechanic<Stat_Targeting>().additionalTargets;
        if (numberOfTargets >= 0 && targets.Count > actualNumberOfTargets)
            targets.RemoveRange(actualNumberOfTargets, targets.Count - actualNumberOfTargets);

        if (targets.Count > 0)
            primaryTarget = targets[0];
        return targets;
    }

    protected override int SortTargets(Entity e1, Entity e2)
    {
        int comparison = 0;
        float e1Assist = Owner.GetMechanic<Stat_PlayerOwner>().player.killedBy[e1.GetMechanic<Stat_PlayerOwner>().player];
        float e2Assist = Owner.GetMechanic<Stat_PlayerOwner>().player.killedBy[e2.GetMechanic<Stat_PlayerOwner>().player];
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
