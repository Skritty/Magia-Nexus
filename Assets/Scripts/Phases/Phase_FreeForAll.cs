using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TwitchLib.Api.Helix.Models.Extensions.ReleasedExtensions;
using TwitchLib.Api.Helix.Models.Teams;
using System.Runtime.Remoting.Messaging;

[CreateAssetMenu(menuName = "Phases/FreeForAll")]
public class Phase_FreeForAll : Phase_Combat
{
    public int adjacentSpawns = 2;
    public Vector2 minMaxSpawnDistance;
    public override List<EntitySpawns> GetEntitySpawns()
    {
        List<EntitySpawns> spawns = new List<EntitySpawns>();
        float degreesBetween = 360f / adjacentSpawns;
        int i = 0;
        List<Vector3> validPositions = new List<Vector3>();
        validPositions.Add(Vector3.zero);

        // Generate a selection of possible spawn points
        for(i = 0; i < GameManager.Instance.viewers.Count * 2; i++)
        {
            int tries = 0;
            for (float a = 0; a < 360f; a += degreesBetween)
            {
                Vector3 newPosition;
                do
                {
                    float offset = Random.Range(-degreesBetween * 0.5f, degreesBetween * 0.5f) * Mathf.Deg2Rad;
                    newPosition = validPositions[i] + new Vector3(
                        Mathf.Cos(offset + a * Mathf.Deg2Rad),//x=rcos(theta),y=rsin(theta), and z=z
                        Mathf.Sin(offset + a * Mathf.Deg2Rad),
                        0) * Random.Range(minMaxSpawnDistance.x, minMaxSpawnDistance.y);
                } while (tries++ < 20 && validPositions.Exists(e => Vector3.Distance(e, newPosition) < minMaxSpawnDistance.x));
                if (tries > 20) break;
                validPositions.Add(newPosition);
            }
        }

        // Randomize and set player spawns
        validPositions.Sort((x, y) => Random.Range(-1, 2));
        i = 0;
        int team = 0;
        foreach (KeyValuePair<string, Viewer> viewer in GameManager.Instance.viewers)
        {
            spawns.Add(new EntitySpawns(validPositions[i++], viewer.Value, team++));
        }

        return spawns;
    }
}