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
    private System.Action cleanup;

    public override void OnEnter()
    {
        SpawnEntities();
    }

    private void SpawnEntities()
    {
        spawns = GetEntitySpawns();
        remainingPlayers.Clear();
        foreach (EntitySpawns spawn in spawns)
        {
            spawn.owner.killGainMultiplier = killPointGain;
            Entity entity = Instantiate(GameManager.Instance.defaultPlayer, spawn.position, Quaternion.identity);
            entity.Stat<Stat_PlayerOwner>().SetPlayer(spawn.owner, entity);
            spawn.owner.roundPoints = 0;
            entity.Stat<Stat_Team>().team = spawn.team;
            entity.Stat<Stat_Actions>().startingTickDelay = (int)(GameManager.Instance.timePerTurn * 50);
            if (!remainingPlayers.TryAdd(spawn.team, 1)) remainingPlayers[spawn.team]++;
            entity.Stat<Stat_Targeting>().targetingType = spawn.owner.targetType;
            cleanup += Trigger_Die.Subscribe(TrackKill, entity);
            foreach (Item item in spawn.owner.items)
            {
                item.OnGained(entity);
            }
            for (int i = 0; i < spawn.owner.actions.Count; i++)
            {
                entity.Stat<Stat_Actions>().SetAction(spawn.owner.actions[i], i);
            }
        }
    }

    private void TrackKill(Trigger_Die trigger)
    {
        Entity dead = trigger.Effect.Target;
        int team = dead.Stat<Stat_Team>().team;
        remainingPlayers[team]--;
        if (remainingPlayers[team] <= 0) remainingPlayers.Remove(team);
        
        if(remainingPlayers.Count == 1)
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
                }
            }
            cleanup?.Invoke();
            GameManager.Instance.NextPhase();
        }
    }

    public abstract List<EntitySpawns> GetEntitySpawns();
}
