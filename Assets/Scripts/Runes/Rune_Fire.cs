using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Rune/Fire")]
public class Rune_Fire : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public EffectTask buff;
    [SerializeReference]
    public EffectTask debuff;
    [SerializeReference]
    public Effect_Damage magicEffectModifier;
    public float explosionMultiplierPerStack;
    public float explosionRadiusPerStack;

    [Header("Spell Shape")]
    [FoldoutGroup("Circle")]
    public Effect_Damage circleEffect;
    [FoldoutGroup("Circle")]
    public Entity circleProxy;
    [FoldoutGroup("Circle")]
    public int maxStages;
    [FoldoutGroup("Circle")]
    public float multiplierPerStage;
    [FoldoutGroup("Circle")]
    public float circleModMultiplierPerStage;

    [FoldoutGroup("Conjuration")]
    public DamageModifier multiplierPerConjureUse;

    [FoldoutGroup("Line")]
    public float lineMulti;

    [FoldoutGroup("Curse")]
    public Effect_Damage curseExplosion;

    public override void MagicEffect(DamageInstance damage, int currentRuneIndex)
    {
        //damage.postOnHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        if (damage.postOnHitEffects.Length == 0)
        {
            if (damage.runes.Count <= 1) return;

            Effect_Damage explosion = magicEffectModifier.Clone();
            
            Trigger_PostHit.Subscribe(x =>
            {
                int i = x.runes.Count;
                x.runes.AddRange(damage.runes); // TODO: currently pretending this works right
                x.runes.RemoveAt(i);
            }, explosion);

            //damage.postOnHitEffects.Add(explosion);
        }
        else
        {
            (damage.postOnHitEffects[0] as Effect_Damage).hit.EffectMultiplier += explosionMultiplierPerStack;
            ((damage.postOnHitEffects[0] as Effect_Damage).targetSelector as Targeting_Radial).radius += explosionRadiusPerStack;
        }
        
        // TODO: Add delay, add debuffs/other effects?
    }

    public override void Shape(Spell spell, int currentRuneIndex)
    {
        // Create effect
        spell.shape = SpellShape.Circle;
        spell.effect = circleEffect.Clone();
        spell.SetChannelSpell(maxStages);
        spell.cleanup += Trigger_SpellStageIncrement.Subscribe(SpellMultiPerStageFire, spell);
        

        // Create circle Proxy
        Entity proxy = GameObject.Instantiate(circleProxy, spell.Target.transform.position, Quaternion.identity);
        spell.proxies.Add(proxy);
        spell.cleanup += Trigger_SpellFinished.Subscribe(x => { if (proxy != null) GameObject.Destroy(proxy.gameObject); }, spell);
    }

    private void SpellMultiPerStageFire(Spell spell)
    {
        spell.EffectMultiplier += multiplierPerStage;
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.cleanup += Trigger_SpellStageIncrement.Subscribe(_ => spell.EffectMultiplier += circleModMultiplierPerStage, spell);
                    break;
                }
            case SpellShape.Conjuration:
                {
                    spell.cleanup += Trigger_ActionStart.Subscribe(x => spell.cleanup += spell.Owner.Stat<Stat_DamageDealt>().AddModifier(multiplierPerConjureUse), spell.Owner);
                    break;
                }
            case SpellShape.Line:
                {
                    spell.EffectMultiplier += lineMulti;
                    (spell.effect.targetSelector as Targeting_Line).width /= 2;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.cleanup += Trigger_ProjectileCreated.Subscribe(projectile => projectile.Stat<Stat_PiercesRemaining>().AddModifier(2), spell.effect);
                    break;
                }
            case SpellShape.Summon:
                {
                    break;
                }
            case SpellShape.Curse:
                {
                    spell.cleanup += Trigger_ModifierLost.Subscribe(x => CurseExplode(spell, x), spell.effect);
                    break;
                }
        }
    }

    private void CurseExplode(Spell spell, IModifier modifier)
    {
        Effect_Damage explosion = curseExplosion.Clone();
        spell.cleanup += Trigger_PreHit.Subscribe(x => x.runes.AddRange(spell.runes), explosion);
        explosion.DoTask(null, spell.Owner); // TODO: WHERE IS EFFECT IN THESE HOW AM I SUPPOSED TO CHAIN EFFECT MULTIPLIERS
    }
}