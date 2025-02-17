using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Skritty.Tools.Saving;
using UnityEngine;

public class Rune_Water : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public Effect debuff;

    [Header("Spell Shape")]
    [SerializeReference]
    public PE_OverrideActions actionOverride;
    [SerializeReference]
    public Targeting multicastConeTargeting;

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        damage.GenerateMagicEffect(damage.runes.Where(x => x.element != RuneElement.Water).ToList());
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Conjuration;
        spell.effect = actionOverride.Clone();
        spell.cleanup += Trigger_PreHit.Subscribe(x => AddMagicEffectRunesToAttackDamage(spell, x.Damage), spell.Owner, -5);
    }

    private void AddMagicEffectRunesToAttackDamage(Spell spell, DamageInstance damage)
    {
        foreach(EffectModifier modifier in damage.damageModifiers)
        {
            if (modifier.calculationType == EffectModifierCalculationType.Flat && modifier.damageType.HasFlag(DamageType.Attack))
            {
                damage.runes.AddRange(spell.runes);
                return;
            }
        }
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    if((spell.effect.targetSelector as Targeting_Radial).angle == 180)
                    {
                        (spell.effect.targetSelector as Targeting_Radial).angle = 30f;
                        (spell.effect.targetSelector as Targeting_Radial).radius 
                            = (spell.effect.targetSelector as Targeting_Radial).radius * 2;
                    }
                    //spell.castSpell.spawnOnTarget = false;
                    (spell.effect.targetSelector as Targeting_Radial).angle += 15f;
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    //spell.castSpell.targetSelector = multicastConeTargeting;
                    //spell.castTargets += 2;
                    break;
                }
            case SpellShape.Projectile:
                {
                    //spell.castSpell.numberOfProjectiles += 4;
                    //spell.multiplier = 1f / spell.castSpell.numberOfProjectiles;
                    //spell.castSpell.projectileFanType = CreateEntity.ProjectileFanType.Shotgun;
                    //spell.castSpell.projectileFanAngle += 30f;
                    break;
                }
            case SpellShape.Summon:
                {
                    break;
                }
            case SpellShape.Curse:
                {
                    
                    break;
                }
        }
    }
}