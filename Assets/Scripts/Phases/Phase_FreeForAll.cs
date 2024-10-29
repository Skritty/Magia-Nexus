using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Phases/FreeForAll")]
public class Phase_FreeForAll : Phase_Combat
{
    public Vector2 minMaxSpawnDistance;
    public override List<EntitySpawns> GetEntitySpawns()
    {
        List<EntitySpawns> spawns = new List<EntitySpawns>();
        float spawnRadius = 0;
        Vector3 center = Vector3.zero;
        Vector3 spawnPoint = Vector3.zero;
        int team = 0;
        int tries = 0;
        foreach (KeyValuePair<string, Viewer> viewer in GameManager.Instance.viewers)
        {
            int i = 0;
            foreach(EntitySpawns spawn in spawns)
            {
                center += spawn.position;
                i++;
            }
            if(i > 0) center = center / i;

            do
            {
                spawnPoint = center + new Vector3(
                Mathf.Cos(Random.Range(0, Mathf.PI * 2)),//x=rcos(theta),y=rsin(theta), and z=z
                Mathf.Cos(Random.Range(0, Mathf.PI * 2)),
                0) * Random.Range(0, spawnRadius);
            } while (tries++ < 10000 && spawns.Exists(e => Vector3.Distance(e.position, spawnPoint) < minMaxSpawnDistance.x));
            spawnRadius += minMaxSpawnDistance.y;
            spawns.Add(new EntitySpawns(spawnPoint, viewer.Value, ++team));
        }

        return spawns;
    }
}