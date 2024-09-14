using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Phases/1v1")]
public class Phase_1v1 : Phase_Combat
{
    public override List<EntitySpawns> GetEntitySpawns()
    {
        List<EntitySpawns> spawns = new List<EntitySpawns>();
        int count = 0;
        int team = 0;
        foreach (KeyValuePair<string, Viewer> viewer in GameManager.Instance.viewers)
        {
            spawns.Add(new EntitySpawns(Vector3.up * 4 * count++, viewer.Value, (team++)));// % 2)));
        }

        return spawns;
    }
}