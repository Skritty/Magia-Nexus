using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Rune_Chaos : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public PersistentEffect debuff;

    [Header("Spell Shape")]
    [FoldoutGroup("Curse")]
    public DamageInstance curseHit;
    [FoldoutGroup("Curse")]
    public PE_PreventExpire curseEffect;
    [FoldoutGroup("Curse")]
    public float baseChanneledCurseEffect;

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        for(int i = 0; i < 2; i++)
        {
            Rune pick = damage.runes[Random.Range(0, damage.runes.Count)];
            int option = Random.Range(0, 2);
            if(pick.GetType() == typeof(Rune_Chaos))
            {
                option++; // Ensure we can't pick chaos modifier again
            }
            if(option == 0)
            {
                // Modifier
                pick.MagicEffectModifier(damage, currentRuneIndex);
            }
            else
            {
                // Buff/Debuff
                pick.MagicEffect(damage);
            }
        }
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Curse;
        spell.effect = curseHit.Clone();
        spell.AddRunesToDamageInstance(spell.effect as DamageInstance);
        (spell.effect as DamageInstance).onHitEffects.Add(curseEffect.Clone());
        spell.proxies.Add(spell.Owner);
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.SetCastOnStageGained();
                    spell.maxStages++;
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    (spell.effect.targetSelector as Targeting_Line).numberOfTargets += 1;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.cleanup += Trigger_ProjectileCreated.Subscribe(AddHomingToProjectile, spell.effect);
                    break;
                }
            case SpellShape.Summon:
                {
                    break;
                }
            case SpellShape.Curse:
                {
                    if (!spell.channeled)
                    {
                        spell.effect.effectMultiplier = baseChanneledCurseEffect;
                        spell.SetChannelSpell(0);
                        spell.SetCastOnStageGained();
                    }
                    spell.maxStages += 2;
                    break;
                }
        }
    }

    private void AddHomingToProjectile(Trigger_ProjectileCreated trigger)
    {
        if (!(trigger.Entity.Stat<Stat_Movement>().movementSelector is Movement_HomeToTarget))
        {
            trigger.Entity.Stat<Stat_Movement>().movementSelector = new Movement_HomeToTarget();
        }
        (trigger.Entity.Stat<Stat_Movement>().movementSelector as Movement_HomeToTarget).homingRateDegreesPerSecond += 30f;
    }
}
