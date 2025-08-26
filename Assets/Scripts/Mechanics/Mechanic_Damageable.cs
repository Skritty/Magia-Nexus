using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_DamageOverTime : ListStat<float>, IStat<float> { }
public class Stat_DamageDealt : ListStat<float>, IStat<float> { }
public class Stat_DamageTaken : ListStat<float>, IStat<float> { }
public class Stat_Invulnerable : BooleanPrioritySolver, IStat<bool> { }
public class Stat_CurrentLife : StepCalculation, IStat<float> { }
public class Stat_MaxLife : StepCalculation, IStat<float> { }
public class Stat_Counter_DamageDealt : Stat_Counter, IStat<int> { }
public class Mechanic_Damageable : Mechanic<Mechanic_Damageable>
{
    #region Internal Variables
    [SerializeField, FoldoutGroup("@GetType()"), ShowIf("@Owner != null")]
    private RectTransform healthBar;
    [SerializeField, FoldoutGroup("@GetType()"), ShowIf("@Owner != null")]
    public VFX_TextPopup damagePopup;
    private DataContainer<float> baseLife;
    #endregion

    public static float TankContributionMultiplier = 0.3f;

    public override void Initialize()
    {
        base.Initialize();
        baseLife = new DataContainer<float>();
        baseLife.Value = Owner.Stat<Stat_MaxLife>().Value;
        Owner.Stat<Stat_CurrentLife>().Add(baseLife);
        Owner.cleanup += Trigger_Die.Subscribe(x => Debug.Log($"{x.Target} DIED"), Owner);
    }

    public override void Tick()
    {
        HandleDoTDamage();
    }

    private void HandleDoTDamage()
    {
        // TODO: damage dealt mods won't apply like this
        DamageInstance damage = new DamageInstance();
        damage.EffectMultiplier = 1;
        damage.Target = Owner;
        foreach (IDataContainer<float> modifier in Owner.Stat<Stat_DamageOverTime>().Modifiers)
        {
            damage.damageModifiers.Add(modifier as Modifier_Damage);
        }
        TakeDamage(damage, true);
    }

    public void TakeDamage(DamageInstance damage, bool triggered = false)
    {
        if (Owner.Stat<Stat_CurrentLife>().Value == 0 || damage.EffectMultiplier == 0 || damage.Target == null || !damage.Target.gameObject.activeSelf)
        {
            return;
        }

        DamageCalculation calculation = new DamageCalculation();
        foreach(Modifier_Damage d in damage.damageModifiers)
        {
            calculation.Add(d);
        }
        if(damage.Owner != null)
        foreach (Modifier_Damage d in damage.Owner.Stat<Stat_DamageDealt>().Modifiers)
        {
            calculation.Add(d);
        }
        foreach (Modifier_Damage d in damage.Target.Stat<Stat_DamageTaken>().Modifiers)
        {
            calculation.Add(d);
        }
        damage.finalDamage = calculation.Value;
        if (damage.Owner != null)
            damage.Owner.Stat<Stat_Counter_DamageDealt>()[damage.Target] += (int)damage.finalDamage;
        damage.Target.Stat<Stat_Counter_DamageDealt>()[damage.Target] += (int)damage.finalDamage;
        //Debug.Log($"Dealing {damage.finalDamage} damage from {damage.Owner} to {damage.Target} ({Owner}). Moving from {baseLife.Value}hp to {baseLife.Value-damage.finalDamage}");

        /*// Apply tank contribution
        if (damage.finalDamage > 0)
            Owner.GetMechanic<Mechanic_PlayerOwner>().ApplyContribution(damage.owner, damage.calculatedDamage / 100f * TankContributionMultiplier);*/

        baseLife.Value = Mathf.Clamp(baseLife.Value - damage.finalDamage, float.MinValue, Owner.Stat<Stat_MaxLife>().Value);
        Owner.Stat<Stat_CurrentLife>().MarkAsChanged();

        Entity triggerOwner = damage.triggerPlayerCharacter ? damage.Owner.GetMechanic<Mechanic_PlayerOwner>().proxyOwner : damage.Owner;
        if (!triggered && damage.finalDamage != 0)
        {
            damagePopup?.PlayVFX<VFX_TextPopup>(Owner.transform, Vector3.up * 0.6f, Vector3.up, false)
            ?.ApplyPopupInfo(Mathf.Round(damage.finalDamage).ToString("0"), new Color(.9f, .2f, .2f));

            Trigger_Damage.Invoke(damage, damage, damage.Owner, triggerOwner, damage.Target);
        }

        if (Owner.Stat<Stat_CurrentLife>().Value <= 0)
        {
            Owner.GetMechanic<Mechanic_PlayerOwner>().DistributeRewards();
            Trigger_Die.Invoke(damage, damage, damage.Owner, triggerOwner, Owner);
            Trigger_Expire.Invoke(Owner, Owner);
        }

        healthBar.localScale = new Vector3(Owner.Stat<Stat_CurrentLife>().Value / Owner.Stat<Stat_MaxLife>().Value, 1, 1);
    }
}