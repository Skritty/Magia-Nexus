using System;
using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Extensions.ReleasedExtensions;
using UnityEngine;

public abstract class Phase_Combat : Phase
{
    public class EntitySpawns
    {
        public Vector3 position;
        public Viewer owner;
        public int team;

        public EntitySpawns(Vector3 position, Viewer viewer, int team)
        {
            this.position = position;
            this.owner = viewer;
            this.team = team;
        }
    }

    public int winPointGain;
    public int killPointGain;
    private Dictionary<int, int> remainingPlayers = new Dictionary<int, int>();
    public List<EntitySpawns> spawns;
    public List<Spell> globalSpell = new List<Spell>();
    private System.Action cleanup;

    public override void OnEnter()
    {
        base.OnEnter();
        SpawnEntities();
    }

    public override void OnExit()
    {
        foreach (Viewer player in GameManager.Viewers)
        {
            Trigger_RoundEnd.Invoke(player, player);
        }
        base.OnExit();
    }

    private void SpawnEntities()
    {
        spawns = GetEntitySpawns();
        remainingPlayers.Clear();
        foreach (EntitySpawns spawn in spawns)
        {
            spawn.owner.killGainMultiplier = killPointGain;
            Entity entity = Instantiate(GameManager.Instance.defaultPlayer, spawn.position, Quaternion.identity);
            entity.GetMechanic<Mechanic_PlayerOwner>().SetPlayer(spawn.owner, entity);
            spawn.owner.roundPoints = 0;
            entity.Stat<Stat_Team>().AddModifier(spawn.team);
            entity.Stat<Stat_Initiative>().AddModifier((int)(GameManager.Instance.timePerTurn * 50));
            if (!remainingPlayers.TryAdd(spawn.team, 1)) remainingPlayers[spawn.team]++;
            entity.Stat<Stat_TargetingMethod>().AddModifier(spawn.owner.personality.targeting);
            entity.Stat<Stat_MovementTargetingMethod>().AddModifier(spawn.owner.personality.targeting);
            cleanup += Trigger_Die.Subscribe(TrackKill, entity);
            foreach (Item item in spawn.owner.items)
            {
                item.OnGained(entity);
            }
            int turnActionCount = GameManager.Instance.defaultActionsPerTurn;
            foreach (Item item in spawn.owner.items)
            {
                turnActionCount += item.actionCountModifier;
            }
            entity.GetMechanic<Mechanic_Actions>().actionsPerTurn = turnActionCount;
            for (int i = 0; i < turnActionCount; i++)
            {
                entity.GetMechanic<Mechanic_Actions>().AddAction(spawn.owner.actions[i]);
            }
        }
    }

    private void TrackKill(DamageInstance damage)
    {
        Entity dead = damage.Target;
        int team = dead.Stat<Stat_Team>().Value;
        if (!remainingPlayers.ContainsKey(team)) return;
        remainingPlayers[team]--;
        if (remainingPlayers[team] <= 0) remainingPlayers.Remove(team);

        dead.GetMechanic<Mechanic_PlayerOwner>().player.deaths++;

        if (remainingPlayers.Count == 1)
        {
            int winningTeam = 0;
            foreach (KeyValuePair<int, int> t in remainingPlayers) winningTeam = t.Key;
            foreach (EntitySpawns spawn in spawns)
            {
                if(spawn.team == winningTeam)
                {
                    spawn.owner.points += winPointGain;
                    spawn.owner.gold += winPointGain;
                    spawn.owner.totalGold += winPointGain;
                    spawn.owner.wins++;
                    spawn.owner.winstreak++;
                }
                else
                {
                    spawn.owner.losses++;
                    spawn.owner.winstreak = 0;
                }
            }
            cleanup?.Invoke();
            GameManager.Instance.NextPhase();
        }
    }

    public abstract List<EntitySpawns> GetEntitySpawns();
}
