using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TwitchLib.Api.Helix.Models.Common;
using Unity.VisualScripting;
using static UnityEngine.EventSystems.EventTrigger;

public class Rune_Fire : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public TriggeredEffect buff;
    [SerializeReference]
    public PersistentEffect debuff;
    [SerializeReference]
    public DamageInstance magicEffectModifier;

    [Header("Spell Shape")]
    public int maxStages;
    public float multiplierPerStage;
    public float circleModMultiplierPerStage;
    [SerializeReference]
    public Targeting shape;

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        DamageInstance explosion = magicEffectModifier.Clone();
        damage.onHitEffects.Add(explosion);
        // TODO: Add delay, add debuffs/other effects?
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Circle;
        spell.effect.targetSelector = shape;
        spell.castSpell.spawnOnTarget = true;
        spell.lifetime = -1;
        spell.channeled = true;
        spell.entity.Stat<Stat_Magic>().maxStages = maxStages;
        spell.entity.Subscribe<Trigger_OnSpellStageIncrement>(_ => spell.multiplier += multiplierPerStage); // TODO: Use the entity proper
        spell.entity.Subscribe<Trigger_OnSpellMaxStage>(_ => spell.multiplier += multiplierPerStage);
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.entity.Subscribe<Trigger_OnSpellStageIncrement>(_ => spell.multiplier += circleModMultiplierPerStage);
                    spell.entity.Subscribe<Trigger_OnSpellMaxStage>(_ => spell.multiplier += circleModMultiplierPerStage);
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    spell.aoeTargetsModifier += 1;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.entity.Stat<Stat_Projectile>().piercesRemaining += 2;
                    break;
                }
            case SpellShape.Summon:
                {
                    break;
                }
            case SpellShape.Curse:
                {
                    spell.castTargets += 1;
                    break;
                }
        }
    }
}