using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_Projectile : Effect_CreateEntity
{
    public enum ProjectileFanType
    {
        EvenlySpaced = 1,
        Sequence = 2,
        Shotgun = 4,
    }

    [FoldoutGroup("Projectile Behavior")]
    public int numberOfProjectiles = 1;
    [FoldoutGroup("Projectile Behavior")]
    public bool ignoreAdditionalProjectiles;
    [FoldoutGroup("Projectile Behavior")]
    public ProjectileFanType projectileFanType = ProjectileFanType.Sequence;
    [FoldoutGroup("Projectile Behavior"), Range(0f, 180f)]
    public float projectileFanAngle = 45;

    protected override IEnumerator SpawnEntity(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        int projectileCount = numberOfProjectiles + (ignoreAdditionalProjectiles ? 0 : (int)Stats.GetStat<Stat_Projectiles>(Owner).Value);
        for (int i = 0; i < projectileCount; i++)
        {
            Entity spawnedEntity = Create(Owner, Target, multiplier, triggered, i);
            projectileFanAngle = Mathf.Clamp(projectileFanAngle, 0f, 180f);
            if (projectileCount > 1)
                switch (projectileFanType)
                {
                    case ProjectileFanType.EvenlySpaced:
                        {
                            spawnedEntity.GetStat<Mechanic_Movement>().facingDir =
                                Quaternion.Euler(0, 0,
                                Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                i * 1f / (projectileCount - 1)))
                                * spawnedEntity.GetStat<Mechanic_Movement>().facingDir;
                            break;
                        }
                    case ProjectileFanType.Shotgun:
                        {
                            spawnedEntity.GetStat<Mechanic_Movement>().facingDir =
                                Quaternion.Euler(0, 0,
                                Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                UnityEngine.Random.Range(0f, 1f)))
                                * spawnedEntity.GetStat<Mechanic_Movement>().facingDir;
                            break;
                        }
                        /*case ProjectileFanType.Sequence:
                            {
                                int tickDelay = (int)(1f * Owner.GetStat<Mechanic_Skills>().TicksPerAction / projectileCount);
                                for (int tick = 0; tick < tickDelay; tick++)
                                {
                                    yield return new WaitForFixedUpdate();
                                }
                                break;
                            }*/
                }
            yield return new WaitForFixedUpdate(); // REMOVE THIS
            spawnedEntity.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.GetStat<Mechanic_Movement>().facingDir);
            Trigger_ProjectileCreated.Invoke(spawnedEntity, spawnedEntity, entity, this, Owner);
        }
    }
}
