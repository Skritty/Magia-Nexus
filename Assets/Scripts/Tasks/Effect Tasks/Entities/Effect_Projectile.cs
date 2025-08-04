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
        int projectileCount = numberOfProjectiles + (ignoreAdditionalProjectiles ? 0 : (int)Owner.Stat<Stat_Projectiles>().Value);
        for (int i = 0; i < projectileCount; i++)
        {
            Entity spawnedEntity = Create(Owner, Target, multiplier, triggered, i);
            projectileFanAngle = Mathf.Clamp(projectileFanAngle, 0f, 180f);
            if (projectileCount > 1)
                switch (projectileFanType)
                {
                    case ProjectileFanType.EvenlySpaced:
                        {
                            spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir =
                                Quaternion.Euler(0, 0,
                                Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                i * 1f / (projectileCount - 1)))
                                * spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir;
                            break;
                        }
                    case ProjectileFanType.Shotgun:
                        {
                            spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir =
                                Quaternion.Euler(0, 0,
                                Mathf.Lerp(-projectileFanAngle, projectileFanAngle,
                                UnityEngine.Random.Range(0f, 1f)))
                                * spawnedEntity.GetMechanic<Mechanic_Movement>().facingDir;
                            break;
                        }
                    case ProjectileFanType.Sequence:
                        {
                            int tickDelay = (int)(1f * Owner.GetMechanic<Mechanic_Actions>().TicksPerAction / projectileCount);
                            for (int tick = 0; tick < tickDelay; tick++)
                            {
                                yield return new WaitForFixedUpdate();
                            }
                            break;
                        }
                }
            spawnedEntity.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.GetMechanic<Mechanic_Movement>().facingDir);
            Trigger_ProjectileCreated.Invoke(spawnedEntity, spawnedEntity, entity, this, Owner);
        }
    }
}
