using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Teams;
using UnityEngine;

[CreateAssetMenu(menuName = "Phases/TeamVS")]
public class Phase_TeamVS : Phase_Combat
{
    public int teamCount = 2;
    public float teamDistFromCenter = 10f;
    public int adjacentSpawns = 2;
    public Vector2 minMaxSpawnDistance;
    public override List<EntitySpawns> GetEntitySpawns()
    {
        List<EntitySpawns> spawns = new List<EntitySpawns>();
        float degreesBetweenTeams = 360f / teamCount;
        float degreesBetweenSpawns = 360f / adjacentSpawns;
        int i = 0;
        List<List<Vector3>> teamSpawnPositions = new List<List<Vector3>>();

        // Generate a selection of possible spawn points
        for (float t = 0; t < 360f; t += degreesBetweenTeams)
        {
            List<Vector3> validPositions = new List<Vector3>();
            
            Vector3 teamOffset = new Vector3(
                Mathf.Cos(t * Mathf.Deg2Rad),//x=rcos(theta),y=rsin(theta), and z=z
                Mathf.Sin(t * Mathf.Deg2Rad),
                0) * teamDistFromCenter;
            validPositions.Add(teamOffset);

            for (i = 0; i < GameManager.Instance.viewers.Count * 2f / teamCount; i++)
            {
                int tries = 0;
                for (float a = 0; a < 360f; a += degreesBetweenSpawns)
                {
                    Vector3 newPosition;
                    do
                    {
                        float offset = Random.Range(-degreesBetweenSpawns * 0.5f, degreesBetweenSpawns * 0.5f) * Mathf.Deg2Rad;
                        newPosition = validPositions[i] + new Vector3(
                            Mathf.Cos(offset + a * Mathf.Deg2Rad),//x=rcos(theta),y=rsin(theta), and z=z
                            Mathf.Sin(offset + a * Mathf.Deg2Rad),
                            0) * Random.Range(minMaxSpawnDistance.x, minMaxSpawnDistance.y);
                    } while (tries++ < 20 && validPositions.Exists(e => Vector3.Distance(e, newPosition) < minMaxSpawnDistance.x));
                    if (tries > 20) break;
                    validPositions.Add(newPosition);
                }
            }

            validPositions.Sort((x, y) => Random.Range(-1, 2));
            teamSpawnPositions.Add(validPositions);
        }

        // Randomize and set player spawns
        i = 0;
        foreach (KeyValuePair<string, Viewer> viewer in GameManager.Instance.viewers)
        {
            spawns.Add(new EntitySpawns(teamSpawnPositions[i % teamCount][(i / teamCount)], viewer.Value, i % teamCount));
            i++;
        }

        return spawns;
    }
}