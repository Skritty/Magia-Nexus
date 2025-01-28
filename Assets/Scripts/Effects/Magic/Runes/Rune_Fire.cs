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
    public PE_Trigger buff;
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
        spell.cleanup += Trigger_OnSpellStageIncrement.Subscribe(x => SpellMultiPerStage(spell, x.entity, multiplierPerStage));
        spell.cleanup += Trigger_OnSpellMaxStage.Subscribe(x => SpellMultiPerStage(spell, x.entity, multiplierPerStage));
    }

    private void SpellMultiPerStage(Spell spell, Entity entity, float multi)
    {
        if (spell != entity.Stat<Stat_Magic>().originSpell) return;
        foreach(Action action in entity.Stat<Stat_Actions>().actions)
        {
            action.effectMultiplier = 1 + entity.Stat<Stat_Magic>().Stage * multi;
        }
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.cleanup += Trigger_OnSpellStageIncrement.Subscribe(x => SpellMultiPerStage(spell, x.entity, circleModMultiplierPerStage));
                    spell.cleanup += Trigger_OnSpellMaxStage.Subscribe(x => SpellMultiPerStage(spell, x.entity, circleModMultiplierPerStage));
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