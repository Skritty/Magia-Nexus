using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int defaultConjurationTickDuration;
    [SerializeReference]
    public PE_OverrideActions actionOverride;
    [SerializeReference]
    public PE_Trigger grantRunesToAttacks;
    [SerializeReference]
    public PE_Trigger grantRunesToProjectiles;
    [SerializeReference]
    public PE_EffectModifer expandingAoEBuff;
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
        spell.spellAction.effects.Add(actionOverride);
        List<Rune> magicEffectRunes = new List<Rune>();
        magicEffectRunes.AddRange(spell.runes);
        magicEffectRunes.RemoveAt(0);
        spell.entity.Stat<Stat_Magic>().runes = magicEffectRunes;

        // TODO: destroy magic effect rune reference spell after action uses
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
                    spell.castSpell.spawnOnTarget = false;
                    (spell.effect.targetSelector as Targeting_Radial).angle += 15f;
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    spell.castSpell.targetSelector = multicastConeTargeting;
                    spell.castTargets += 2;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.castSpell.numberOfProjectiles += 4;
                    //spell.multiplier = 1f / spell.castSpell.numberOfProjectiles;
                    spell.castSpell.projectileFanType = CreateEntity.ProjectileFanType.Shotgun;
                    spell.castSpell.projectileFanAngle += 30f;
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