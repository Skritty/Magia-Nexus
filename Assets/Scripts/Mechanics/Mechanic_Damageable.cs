using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechanic_Damageable : Mechanic<Mechanic_Damageable>
{
    #region Internal Variables
    [SerializeField, FoldoutGroup("@GetType()"), ShowIf("@Owner != null")]
    private RectTransform healthBar;
    [SerializeField, FoldoutGroup("@GetType()"), ShowIf("@Owner != null")]
    public VFX_TextPopup damagePopup;
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
        if (Owner.Stat<Stat_CurrentLife>().Value == 0 || damage.effectMultiplier == 0)
        {
            return;
        }

        // Make a damage calculation damage.Owner.Stat<Stat_EffectModifiers>()
        DamageModifier calculation = DamageModifier.CreateCalculation(damage);
        damage.AddModifiers(calculation);
        damage.Owner.GetMechanic<Stat_EffectModifiers>().AddModifiersToCalculation(calculation, EffectTag.DamageDealt);
        damage.Target.GetMechanic<Stat_EffectModifiers>().AddModifiersToCalculation(calculation, EffectTag.DamageTaken);
        calculation.Solve();
        damage.calculatedDamage = calculation.Value;

        // Handle Life
        if (damage.calculatedDamage > 0)
            Owner.GetMechanic<Stat_PlayerOwner>().ApplyContribution(damage.Owner, damage.calculatedDamage / 100f * TankContributionMultiplier);
        //Debug.Log($"Dealing {totalDamage} damage from {damage.Owner} to {damage.Target} ({Owner})");
        Owner.Stat<Stat_CurrentLife>().Value = Mathf.Clamp(Owner.Stat<Stat_CurrentLife>().Value - damage.calculatedDamage, 0, Owner.Stat<Stat_MaxLife>().Value);

        Entity triggerOwner = damage.triggerPlayerOwner ? damage.Owner.GetMechanic<Stat_PlayerOwner>().Owner : damage.Owner;
        if (!damage.preventTriggers || damage.calculatedDamage == 0)
        {
            damagePopup?.PlayVFX<VFX_TextPopup>(Owner.transform, Vector3.up * 0.6f, Vector3.up, false)
            ?.ApplyPopupInfo(Mathf.Round(damage.calculatedDamage).ToString("0"), new Color(.9f, .2f, .2f));

            new Trigger_Damage(damage, damage, damage.Owner, triggerOwner, damage.Target);
        }

        if (Owner.Stat<Stat_CurrentLife>().Value <= 0)
        {
            Owner.GetMechanic<Stat_PlayerOwner>().DistributeRewards();
            new Trigger_Die(damage, damage, damage.Owner, triggerOwner, Owner);
        }

        healthBar.localScale = new Vector3(Owner.Stat<Stat_CurrentLife>().Value / Owner.Stat<Stat_MaxLife>().Value, 1, 1);
    }
}