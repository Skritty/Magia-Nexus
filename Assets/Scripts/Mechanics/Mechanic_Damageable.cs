using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_DamageOverTime : ListStat<IDataContainer<float>>, IStatTag { }
public class Stat_DamageDealt : ListStat<IDataContainer<float>>, IStatTag { }
public class Stat_DamageTaken : ListStat<IDataContainer<float>>, IStatTag { }
public class Stat_Invulnerable : BooleanPrioritySolver, IStatTag { }
public class Stat_CurrentLife : NumericalStepCalculationSolver, IStatTag { }
public class Stat_MaxLife : NumericalStepCalculationSolver, IStatTag { }
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

    protected override void Initialize()
    {
        base.Initialize();
        baseLife = new DataContainer<float>();
        baseLife.Value = Owner.Stat<Stat_MaxLife>().Value;
        Owner.Stat<Stat_CurrentLife>().AddModifier(baseLife);
        Owner.cleanup += Trigger_Die.Subscribe(x => Debug.Log($"{x.Target} DIED"), Owner);
    }

    public void TakeDamage(DamageInstance damage)
    {
        if (Owner.Stat<Stat_CurrentLife>().Value == 0 || damage.EffectMultiplier == 0)
        {
            return;
        }

        DamageTypeCalculationSolver calculation = new DamageTypeCalculationSolver();
        foreach(DamageSolver d in damage.damageModifiers)
        {
            calculation.AddModifier(d);
        }
        foreach (IDataContainer d in damage.Owner.Stat<Stat_DamageDealt>().Modifiers)
        {
            calculation.AddModifier(d);
        }
        foreach (IDataContainer d in damage.Target.Stat<Stat_DamageTaken>().Modifiers)
        {
            calculation.AddModifier(d);
        }
        damage.finalDamage = calculation.Value;
        //Debug.Log($"Dealing {totalDamage} damage from {damage.Owner} to {damage.Target} ({Owner})");

        /*// Apply tank contribution
        if (damage.finalDamage > 0)
            Owner.GetMechanic<Mechanic_PlayerOwner>().ApplyContribution(damage.owner, damage.calculatedDamage / 100f * TankContributionMultiplier);*/

        baseLife.Value = Mathf.Clamp(baseLife.Value - damage.finalDamage, float.MinValue, Owner.Stat<Stat_MaxLife>().Value);

        Entity triggerOwner = damage.triggerPlayerOwner ? damage.Owner.GetMechanic<Mechanic_PlayerOwner>().Owner : damage.Owner;
        if (!damage.preventTriggers || damage.finalDamage == 0)
        {
            damagePopup?.PlayVFX<VFX_TextPopup>(Owner.transform, Vector3.up * 0.6f, Vector3.up, false)
            ?.ApplyPopupInfo(Mathf.Round(damage.finalDamage).ToString("0"), new Color(.9f, .2f, .2f));

            Trigger_Damage.Invoke(damage, damage, damage.Owner, triggerOwner, damage.Target);
        }

        if (Owner.Stat<Stat_CurrentLife>().Value <= 0)
        {
            Owner.GetMechanic<Mechanic_PlayerOwner>().DistributeRewards();
            Trigger_Die.Invoke(damage, damage, damage.Owner, triggerOwner, Owner);
        }

        healthBar.localScale = new Vector3(Owner.Stat<Stat_CurrentLife>().Value / Owner.Stat<Stat_MaxLife>().Value, 1, 1);
    }
}