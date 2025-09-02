using Sirenix.OdinInspector;

public class Stat_Team : PrioritySolver<int>, IStat<int> { }
public class Stat_SummonCount : NumericalSolver, IStat<float> { }
public class Stat_Summons : ListStat<Entity>, IStat<Entity> { }
public class Stat_Proxies : ListStat<Entity>, IStat<Entity> { }
public class Stat_TeamPlayers : NumericalSolver, IStat<float> { }
public class Stat_MaxSummons : NumericalSolver, IStat<float> { }
public class Stat_PlayerCharacter : Stat<Entity>, IStat<Entity> { }
public class Stat_LastKilledBy : Stat<Entity>, IStat<Entity> { }
public class Stat_Viewer : Stat<Viewer>, IStat<Viewer> { }
public class Mechanic_Assists : DictionaryStat<Viewer, Entity>, IStat<Entity> { }
public class Mechanic_Character : Mechanic<Mechanic_Character>
{
    [FoldoutGroup("Character")]
    public TMPro.TextMeshProUGUI characterNamePlate;

    public void SetViewer(Viewer viewer)
    {
        
        viewer.character = Owner;
        Owner.AddModifier<Viewer, Stat_Viewer>(viewer, 0);
        Owner.AddModifier<Entity, Stat_PlayerCharacter>(Owner, 0);
        Owner.name = viewer.viewerName;
        if (characterNamePlate != null)
            characterNamePlate.text = viewer.viewerName;
    }

    /*public void ApplyContribution(Entity target, float effectMultiplier)
    {
        if (playerEntity == target) return;
        target = target.GetMechanic<Mechanic_Character>().playerEntity;
        Viewer player = playerEntity.GetMechanic<Mechanic_Character>().player;
        if (!target.GetMechanic<Mechanic_Character>().assists.TryAdd(player, Mathf.Abs(effectMultiplier)))
        {
            target.GetMechanic<Mechanic_Character>().assists[player] += Mathf.Abs(effectMultiplier);
        }
        //Debug.Log($"{playerEntity.name} gaining {effectMultiplier} contribution points towards the kill on {target.name} | Now at {target.Stat<Stat_PlayerOwner>().assists[player]}");
    }

    public void DistributeRewards()
    {
        player.killedBy.Clear();
        player.killedBy.AddRange(assists);
        float totalContribution = 0;
        foreach(KeyValuePair<Viewer, float> assist in assists)
        {
            totalContribution += assist.Value;
        }

        foreach (KeyValuePair<Viewer, float> assist in assists)
        {
            float points = assist.Key.killGainMultiplier * assist.Value / totalContribution;
            if (assist.Value / totalContribution == 0 || assist.Value / totalContribution == float.NaN) Debug.Log(totalContribution);
            assist.Key.points += points;
            assist.Key.roundPoints += points;
        }
    }*/
}