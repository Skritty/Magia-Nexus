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
    [SerializeField, FoldoutGroup("Life"), ShowIf("@Owner != null")]
    public VFX_TextPopup damagePopup;
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
        Owner.cleanup += Trigger_Die.Subscribe((x) => Debug.Log($"{x.Damage.Target} DIED"), Owner);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void TakeDamage(DamageInstance damage)
    {
        if (invulnerable || damage.effectMultiplier == 0)
        {
            return;
        }

        // Make a damage calculation damage.Owner.Stat<Stat_EffectModifiers>()
        DamageModifier calculation = DamageModifier.CreateCalculation(damage);
        damage.AddModifiers(calculation);
        damage.Owner.Stat<Stat_EffectModifiers>().AddModifiersToCalculation(calculation, EffectTag.DamageDealt);
        damage.Target.Stat<Stat_EffectModifiers>().AddModifiersToCalculation(calculation, EffectTag.DamageTaken);
        damage.calculatedDamage = calculation.Solve();

        // Handle Life
        if (damage.calculatedDamage > 0)
            Owner.Stat<Stat_PlayerOwner>().ApplyContribution(damage.Owner, damage.calculatedDamage / 100f * TankContributionMultiplier);
        //Debug.Log($"Dealing {totalDamage} damage from {damage.Owner} to {damage.Target} ({Owner})");
        currentLife = Mathf.Clamp(currentLife - damage.calculatedDamage, 0, maxLife);

        Entity triggerOwner = damage.triggerPlayerOwner ? damage.Owner.Stat<Stat_PlayerOwner>().Owner : damage.Owner;
        if (!damage.preventTriggers || damage.calculatedDamage == 0)
        {
            damagePopup?.PlayVFX<VFX_TextPopup>(Owner.transform, Vector3.up * 0.6f, Vector3.up, false)
            ?.ApplyPopupInfo(Mathf.Round(damage.calculatedDamage).ToString("0"), new Color(.9f, .2f, .2f));

            new Trigger_Damage(damage, damage, damage.Owner, triggerOwner, damage.Target);
        }

        if (currentLife <= 0)
        {
            Owner.Stat<Stat_PlayerOwner>().DistributeRewards();
            new Trigger_Die(damage, damage, damage.Owner, triggerOwner, Owner);
        }

        healthBar.localScale = new Vector3(currentLife / maxLife, 1, 1);
    }
}