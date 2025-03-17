using System.Collections.Generic;
using Sirenix.OdinInspector;
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
    [FoldoutGroup("Circle")]
    public DamageInstance circleEffect;
    [FoldoutGroup("Circle")]
    public Entity circleProxy;
    [FoldoutGroup("Circle")]
    public int maxStages;
    [FoldoutGroup("Circle")]
    public float multiplierPerStage;
    [FoldoutGroup("Circle")]
    public float circleModMultiplierPerStage;

    [FoldoutGroup("Conjuration")]
    public float multiplierPerConjureUse;

    [FoldoutGroup("Line")]
    public float lineMulti;

    [FoldoutGroup("Curse")]
    public DamageInstance curseExplosion;

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        DamageInstance explosion = (DamageInstance)magicEffectModifier.Clone();
        damage.onHitEffects.Add(explosion);
        // TODO: Add delay, add debuffs/other effects?
    }

    public override void Shape(Spell spell)
    {
        // Create effect
        spell.shape = SpellShape.Circle;
        spell.effect = circleEffect.Clone();
        spell.AddRunesToDamageInstance(spell.effect as DamageInstance);
        spell.SetChannelSpell(maxStages);
        spell.cleanup += Trigger_SpellStageIncrement.Subscribe(SpellMultiPerStageFire, spell);
        

        // Create circle Proxy
        Entity proxy = GameObject.Instantiate(circleProxy, spell.spellcast.Target.transform.position, Quaternion.identity);
        spell.proxies.Add(proxy);
        spell.cleanup += Trigger_SpellFinished.Subscribe(x => GameObject.Destroy(proxy.gameObject), spell);
    }

    private void SpellMultiPerStageFire(Trigger_SpellStageIncrement trigger)
    {
        trigger.Spell.effect.effectMultiplier += multiplierPerStage;
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
                    spell.effect.effectMultiplier += lineMulti;
                    (spell.effect.targetSelector as Targeting_Line).width /= 2;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.cleanup += Trigger_ProjectileCreated.Subscribe(x => x.Entity.Stat<Stat_Projectile>().piercesRemaining += 2, spell.effect);
                    break;
                }
            case SpellShape.Summon:
                {
                    break;
                }
            case SpellShape.Curse:
                {
                    spell.cleanup += Trigger_PersistentEffectLost.Subscribe(x => CurseExplode(spell, x.PersistentEffect.Target), spell.effect);
                    break;
                }
        }
    }

    private void SpellMultiPerStageCircle(Trigger_SpellStageIncrement trigger)
    {
        trigger.Spell.effect.effectMultiplier += circleModMultiplierPerStage;
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

    private void CurseExplode(Spell spell, Entity proxy)
    {
        DamageInstance explosion = (DamageInstance)curseExplosion.Clone();
        spell.AddRunesToDamageInstance(explosion);
        explosion.Create(spell.Owner, proxy);
    }
}