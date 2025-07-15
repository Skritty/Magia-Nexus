using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Rune/Fire")]
public class Rune_Fire : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PE_EffectModifer buff;
    [SerializeReference]
    public PersistentEffect debuff;
    [SerializeReference]
    public DamageInstanceOLD magicEffectModifier;

    [Header("Spell Shape")]
    [FoldoutGroup("Circle")]
    public DamageInstanceOLD circleEffect;
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
    public DamageInstanceOLD curseExplosion;

    public override void MagicEffect(DamageInstanceOLD damage, int currentRuneIndex)
    {
        damage.postOnHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstanceOLD damage, int currentRuneIndex)
    {
        if (damage.postOnHitEffects.Count == 0)
        {
            if (damage.runes.Count <= 1) return;

            DamageInstanceOLD explosion = (DamageInstanceOLD)magicEffectModifier.Clone();
            explosion.runes.AddRange(damage.runes);
            explosion.runes.RemoveAt(0);
            
            damage.postOnHitEffects.Add(explosion);
        }
        else
        {
            damage.postOnHitEffects[0].effectMultiplier += magicEffectModifier.effectMultiplier;
            //(damage.postOnHitEffects[0].targetSelector as Targeting_Radial).radius += (magicEffectModifier.targetSelector as Targeting_Radial).radius;
        }
        
        // TODO: Add delay, add debuffs/other effects?
    }

    public override void Shape(Spell spell, int currentRuneIndex)
    {
        // Create effect
        spell.shape = SpellShape.Circle;
        spell.effect = circleEffect.Clone();
        spell.AddRunesToDamageInstance(spell.effect as DamageInstanceOLD);
        spell.SetChannelSpell(maxStages);
        spell.cleanup += Trigger_SpellStageIncrement.Subscribe(SpellMultiPerStageFire, spell);
        

        // Create circle Proxy
        Entity proxy = GameObject.Instantiate(circleProxy, spell.spellcast.Target.transform.position, Quaternion.identity);
        spell.proxies.Add(proxy);
        spell.cleanup += Trigger_SpellFinished.Subscribe(x => { if (proxy != null) GameObject.Destroy(proxy.gameObject); }, spell);
    }

    private void SpellMultiPerStageFire(Spell spell)
    {
        spell.effect.effectMultiplier += multiplierPerStage;
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
                    spell.cleanup += Trigger_ProjectileCreated.Subscribe(x => x.GetMechanic<Mechanic_Projectile>().piercesRemaining += 2, spell.effect);
                    break;
                }
            case SpellShape.Summon:
                {
                    break;
                }
            case SpellShape.Curse:
                {
                    spell.cleanup += Trigger_PersistentEffectLost.Subscribe(x => CurseExplode(x, spell, x.Target), spell.effect);
                    break;
                }
        }
    }

    private void SpellMultiPerStageCircle(Spell spell)
    {
        spell.effect.effectMultiplier += circleModMultiplierPerStage;
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

    private void CurseExplode(PersistentEffect effect, Spell spell, Entity proxy)
    {
        DamageInstanceOLD explosion = (DamageInstanceOLD)curseExplosion.Clone();
        spell.AddRunesToDamageInstance(explosion);
        explosion.CreateFromTrigger(spell.Owner, effect, proxy);
    }
}