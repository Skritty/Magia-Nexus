using System.Collections.Generic;
using UnityEngine;

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
    public float multiplierPerConjureUse;
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
        spell.blueprintEntity.Stat<Stat_Magic>().maxStages = maxStages;
        spell.cleanup += Trigger_SpellStageIncrement.Subscribe(SpellMultiPerStageFire, spell);
    }

    private void SpellMultiPerStageFire(Trigger_SpellStageIncrement trigger)
    {
        foreach(Action action in trigger.Owner.Stat<Stat_Actions>().actions)
        {
            action.effectMultiplier = 1 + trigger.Owner.Stat<Stat_Magic>().Stage * multiplierPerStage;
        }
    }

    private void SpellMultiPerStageCircle(Trigger_SpellStageIncrement trigger)
    {
        foreach (Action action in trigger.Owner.Stat<Stat_Actions>().actions)
        {
            action.effectMultiplier = 1 + trigger.Owner.Stat<Stat_Magic>().Stage * circleModMultiplierPerStage;
        }
    }

    private void ConjureMultiPerUse(Spell spell, Effect e, float multi)
    {
        if (e.Owner == spell.Owner) return;
        /*foreach(KeyValuePair<EffectTag, float> tag in e.effectTags)
        {
            if (tag.Key.HasFlag(EffectTag.Attack | EffectTag.Spell))
            {
                e.effectMultiplier += multi;
                return;
            }
        }*/
        // TODO
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.cleanup += Trigger_SpellStageIncrement.Subscribe(SpellMultiPerStageCircle, spell);
                    break;
                }
            case SpellShape.Conjuration:
                {
                    //spell.cleanup += Trigger_OnActivateEffect.Subscribe(x => ConjureMultiPerUse(spell, x.Effect, multiplierPerConjureUse));
                    break;
                }
            case SpellShape.Line:
                {
                    spell.aoeTargetsModifier += 1;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.blueprintEntity.Stat<Stat_Projectile>().piercesRemaining += 2;
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