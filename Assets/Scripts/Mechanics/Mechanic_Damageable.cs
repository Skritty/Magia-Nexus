using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_DamageOverTime : ListStat<float>, IStat<List<float>> { }
public class Stat_DamageDealt : ListStat<float>, IStat<List<float>> { }
public class Stat_DamageTaken : ListStat<float>, IStat<List<float>> { }
public class Stat_Invulnerable : BooleanPrioritySolver, IStat<bool> { }
public class Stat_CurrentLife : ValueContainer<float>, IStat<float> { }
public class Stat_MaxLife : StepCalculation, IStat<float> { }
public class Mechanic_Damageable : Mechanic
{
    #region Internal Variables
    [SerializeField, FoldoutGroup("@GetType()"), ShowIf("@Owner != null")]
    private RectTransform healthBar;
    [SerializeField, FoldoutGroup("@GetType()"), ShowIf("@Owner != null")]
    public VFX_TextPopup damagePopup;
    #endregion

    public static float TankContributionMultiplier = 0.3f;

    public override void Initialize()
    {
        base.Initialize();
        Owner.GetStat<Stat_CurrentLife>().Value = Owner.GetStat<Stat_MaxLife>().Value;
    }

    public override void Tick()
    {
        HandleDoTDamage();
    }

    private void HandleDoTDamage()
    {
        if (Owner.GetStat<Stat_DamageOverTime>().Modifiers.Count == 0) return;
        // TODO: damage dealt mods won't apply like this
        DamageInstance damage = new DamageInstance();
        damage.EffectMultiplier = 1;
        damage.Target = Owner;
        foreach (IValueContainer<float> modifier in Owner.GetStat<Stat_DamageOverTime>().Modifiers)
        {
            damage.damageModifiers.Add(modifier as Modifier_Damage);
        }
        TakeDamage(damage, true);
    }

    public void TakeDamage(DamageInstance damage, bool triggered = false)
    {
        if (Owner.GetStat<Stat_CurrentLife>().Value <= 0 || damage.EffectMultiplier == 0 || damage.Target == null || !damage.Target.gameObject.activeSelf)
        {
            return;
        }

        DamageCalculation calculation = new DamageCalculation();
        foreach(Modifier_Damage d in damage.damageModifiers)
        {
            calculation.AddModifier(d);
        }
        if(damage.Owner != null)
        foreach (Modifier_Damage d in damage.Owner.GetStat<Stat_DamageDealt>().Modifiers)
        {
            calculation.AddModifier(d);
        }
        foreach (Modifier_Damage d in damage.Target.GetStat<Stat_DamageTaken>().Modifiers)
        {
            calculation.AddModifier(d);
        }
        calculation.AddModifier(new Modifier_Damage(value: damage.EffectMultiplier, step: CalculationStep.Multiplicative, appliesTo: DamageType.All));
        damage.finalDamage = calculation.Value;
        if (damage.Owner != null)
        {
            // Total damage dealt by the owner to the target
            Stats.GetAndCreateStat<Stat_CounterFloat>(damage.Owner, damage.Target, "damageDealt").Value += damage.finalDamage;
            // Total damage dealt by the owner
            Stats.GetAndCreateStat<Stat_CounterFloat>(damage.Owner, "damageDealt").Value += damage.finalDamage;
        }
        // Total damage taken by the target
        Stats.GetAndCreateStat<Stat_CounterFloat>(damage.Target, "damageTaken").Value += damage.finalDamage;

        //Debug.Log($"({Time.time}) Dealing {damage.finalDamage} damage from {damage.Owner} to {damage.Target} ({Owner}). Moving from {baseLife.Value}hp to {baseLife.Value-damage.finalDamage}");

        /*// Apply tank contribution
        if (damage.finalDamage > 0)
            Owner.GetMechanic<Mechanic_PlayerOwner>().ApplyContribution(damage.owner, damage.calculatedDamage / 100f * TankContributionMultiplier);*/

        Owner.GetStat<Stat_CurrentLife>().Value = Mathf.Clamp(Owner.GetStat<Stat_CurrentLife>().Value - damage.finalDamage, 0, Owner.GetStat<Stat_MaxLife>().Value);
        //Owner.GetStat<Stat_CurrentLife>().SetChanged();

        if (!triggered && damage.finalDamage != 0)
        {
            damagePopup?.PlayVFX<VFX_TextPopup>(Owner.transform, Vector3.up * 0.6f, Vector3.up, false)
            ?.ApplyPopupInfo(Mathf.Round(damage.finalDamage).ToString("0"), new Color(.9f, .2f, .2f));

            Trigger_Damage.Invoke(damage, damage, damage.Owner, damage.Target);
        }

        if (Owner.GetStat<Stat_CurrentLife>().Value <= 0)
        {
            //Owner.GetMechanic<Mechanic_Character>().DistributeRewards();
            Trigger_Die.Invoke(damage, damage, damage.Owner, damage.Target);
            Trigger_Expire.Invoke(Owner, Owner);
        }

        healthBar.localScale = new Vector3(Owner.GetStat<Stat_CurrentLife>().Value / Owner.GetStat<Stat_MaxLife>().Value, 1, 1);
    }
}