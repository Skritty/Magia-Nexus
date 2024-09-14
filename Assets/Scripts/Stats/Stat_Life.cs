using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Life : GenericStat<Stat_Life>
{
    #region Internal Variables
    [SerializeField, FoldoutGroup("Life"), ShowIf("@owner != null")]
    private RectTransform healthBar;
    #endregion

    #region Stats
    [FoldoutGroup("Life")]
    public float maxLife;
    [FoldoutGroup("Life")]
    public float currentLife;
    [FoldoutGroup("Life")]
    public bool invulnerable;
    [FoldoutGroup("Life")]
    public List<DamageModifier> damageModifers = new List<DamageModifier>();
    #endregion

    public void TakeDamage(DamageInstance damage)
    {
        // Apply Defenses
        if (invulnerable && damage.damage > 0)
        {
            damage.damage = 0;
        }

        DamageModifier defense = new DamageModifier();
        foreach (DamageModifier d in damageModifers)
        {
            defense.Merge(d);
        }
        defense.ApplyModifiers(damage);

        // Calculate Damage
        damage.owner.Stat<Stat_Damage>().CalculateDamage(damage);

        // Handle Life
        Debug.Log($"{damage.source} dealing {damage.damage} damage from {damage.owner} to {damage.target}");
        currentLife = Mathf.Clamp(currentLife - damage.damage, 0, maxLife);
        if (!damage.preventTriggers)
        {
            owner.Trigger<Trigger_OnDamageRecieved>(damage);
            damage.owner.Trigger<Trigger_OnDamageDealt>(damage);
        }

        if (currentLife <= 0) owner.Trigger<Trigger_OnDie>(damage);

        healthBar.localScale = new Vector3(currentLife / maxLife, 1, 1);
    }
}