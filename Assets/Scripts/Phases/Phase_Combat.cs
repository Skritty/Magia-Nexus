using System.Collections;
using System.Collections.Generic;
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

    public int baseGoldGain;

    public override void OnEnter()
    {
        SpawnEntities();
        GivePlayersGold();
    }

    private void GivePlayersGold()
    {
        foreach(Viewer viewer in GameManager.Viewers)
        {
            viewer.currency += baseGoldGain;
        }
    }

    private void SpawnEntities()
    {
        foreach (EntitySpawns spawns in GetEntitySpawns())
        {
            Entity entity = Instantiate(GameManager.Instance.defaultPlayer, spawns.position, Quaternion.identity);
            entity.Stat<Stat_PlayerOwner>().SetPlayer(spawns.owner, entity);
            entity.Stat<Stat_Team>().team = spawns.team;
            entity.Stat<Stat_Movement>().targetingType = spawns.owner.targetType;
            foreach (Item item in spawns.owner.items)
            {
                item.OnGained(entity);
            }
            for (int i = 0; i < spawns.owner.actions.Count; i++)
            {
                entity.Stat<Stat_Actions>().AddAction(spawns.owner.actions[i], i);
            }
        }
    }

    public abstract List<EntitySpawns> GetEntitySpawns();
}
