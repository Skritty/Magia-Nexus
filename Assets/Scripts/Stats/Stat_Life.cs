using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Life : GenericStat<Stat_Life>
{
    #region Internal Variables
    [SerializeField, FoldoutGroup("Life"), ShowIf("@Owner != null")]
    private RectTransform healthBar;
    #endregion

    #region Stats
    [FoldoutGroup("Life")]
    public float maxLife;
    [FoldoutGroup("Life")]
    public float currentLife;
    [FoldoutGroup("Life")]
    public bool invulnerable;
    #endregion

    public static float TankContributionMultiplier = 0.3f;

    protected override void Initialize()
    {
        base.Initialize();
        Owner.Subscribe<Trigger_OnDie>((x) => Debug.Log($"{x.Data<DamageInstance>().Target} DIED"));
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Owner.Unsubscribe<Trigger_OnDie>((x) => Debug.Log($"{x.Data<DamageInstance>().Target} DIED"));
    }

    public void TakeDamage(DamageInstance damage)
    {
        if (invulnerable)
        {
            return;
        }

        Stat_EffectModifiers.EffectModifier calculation = damage.Owner.Stat<Stat_PlayerOwner>().playerEntity.Stat<Stat_EffectModifiers>().CreateCalculation(null, damage, EffectTag.DamageDealt);
        damage.Target.Stat<Stat_EffectModifiers>().CreateCalculation(calculation, damage, EffectTag.DamageTaken);
        float totalDamage = calculation.CalculateModifier(damage, 1f, TankContributionMultiplier);

        // Handle Life
        if (totalDamage > 0)
            Owner.Stat<Stat_PlayerOwner>().ApplyContribution(damage.Owner, totalDamage * TankContributionMultiplier);
        Debug.Log($"Dealing {totalDamage} damage from {damage.Owner} to {damage.Target} ({Owner})");
        currentLife = Mathf.Clamp(currentLife - totalDamage, 0, maxLife);
        if (!damage.preventTriggers)
        {
            Owner.Trigger<Trigger_OnDamageRecieved>(damage, damage);
            damage.Owner.Stat<Stat_PlayerOwner>().Proxy(x => x.Trigger<Trigger_OnDamageDealt>(damage, damage));
        }

        if (currentLife <= 0)
        {
            Owner.Stat<Stat_PlayerOwner>().DistributeRewards();
            Owner.Trigger<Trigger_OnDie>(damage, damage);
        }

        healthBar.localScale = new Vector3(currentLife / maxLife, 1, 1);
    }
}