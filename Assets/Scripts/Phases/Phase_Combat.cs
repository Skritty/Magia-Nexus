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

    public int minimumPlayers = 2;
    public int winPointGain;
    public int killPointGain;
    private Dictionary<int, int> remainingPlayers = new Dictionary<int, int>();
    public List<EntitySpawns> spawns;
    public List<Spell> globalSpell = new List<Spell>();
    private System.Action cleanup;

    public override void OnEnter()
    {
        base.OnEnter();
        AddCPUs();
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

    private void AddCPUs()
    {
        for(int i = GameManager.Viewers.Length; i < minimumPlayers; i++)
        {
            Viewer cpu = GameManager.Instance.JoinAsCPU();
            cpu.autoplayAI.PurchaseItems(cpu);
            cpu.autoplayAI.CreateTurn(cpu);
            cpu.autoplayAI.SetPersonality(cpu);
        }
    }

    private void SpawnEntities()
    {
        spawns = GetEntitySpawns();
        remainingPlayers.Clear();
        foreach (EntitySpawns spawn in spawns)
        {
            spawn.owner.killGainMultiplier = killPointGain;
            Entity entity = Instantiate(GameManager.Instance.defaultPlayer, spawn.position, Quaternion.identity);
            entity.GetMechanic<Mechanic_Character>().SetViewer(spawn.owner);
            spawn.owner.roundPoints = 0;
            entity.Stat<Stat_Team>().Add(spawn.team);
            if (!remainingPlayers.TryAdd(spawn.team, 1)) remainingPlayers[spawn.team]++;
            spawn.owner.personality.Initialize(entity);
            cleanup += Trigger_Die.Subscribe(x => TrackKill(x.Target, entity), entity);
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
                entity.GetMechanic<Mechanic_Actions>().AddAction(i < spawn.owner.actions.Count ? spawn.owner.actions[i] : null);
            }
        }
    }

    private void TrackKill(Entity dead, Entity owner)
    {
        if (dead != owner) return;
        int team = dead.Stat<Stat_Team>().Value;
        if (!remainingPlayers.ContainsKey(team)) return;
        remainingPlayers[team]--;
        if (remainingPlayers[team] <= 0) remainingPlayers.Remove(team);

        Debug.Log($"{dead} DIED");
        dead.Stat<Stat_Viewer>().Value.deaths++;

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
