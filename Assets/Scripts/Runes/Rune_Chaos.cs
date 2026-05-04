/*using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Rune/Chaos")]
public class Rune_Chaos : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public EffectTask buff;
    [SerializeReference]
    public EffectTask debuff;

    [Header("Spell Shape")]
    [FoldoutGroup("Circle")]
    public float multiplierPerStage;
    [SerializeReference, FoldoutGroup("Projectile")]
    public Movement_HomeToTarget homing;
    [SerializeReference, FoldoutGroup("Curse")]
    public Effect_Hit curseHit;
    [FoldoutGroup("Curse")]
    public float baseChanneledCurseEffect;

    public override void MagicEffect(DamageInstance damage, int currentRuneIndex)
    {
        //damage.postOnHitEffects.Add(debuff);
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
                pick.MagicEffect(damage, damage.runes.IndexOf(pick));
            }
        }
    }

    public override void Shape(Spell spell, int currentRuneIndex)
    {
        spell.shape = SpellShape.Curse;
        spell.effect = curseHit.Clone();
        spell.proxies.Add(spell.Owner);
        spell.SubscribeOnHit(x => Trigger_ModifierLost.Subscribe(x => spell.StopSpell(), x.postOnHitEffects[0]));
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        spell.EffectMultiplier += 0.5f;
        return;
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.cleanup += Trigger_SpellStageIncrement.Subscribe(_ => spell.EffectMultiplier -= multiplierPerStage);
                    spell.SetCastOnStageGained();
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    spell.additionalCastTargets += 1;
                    break;
                }
            case SpellShape.Projectile:
                {
                    spell.cleanup += Trigger_ProjectileCreated.Subscribe(x => AddHomingToProjectile(spell, x), spell.effect);
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
                        spell.EffectMultiplier = baseChanneledCurseEffect;
                        spell.SetChannelSpell(0);
                        spell.SetCastOnStageGained();
                    }
                    spell.maxStages += 2;
                    break;
                }
        }
    }

    private void AddHomingToProjectile(Spell spell, Entity entity)
    {
        if (Stats.GetStat<Stat_MovementSelector>(entity).Value == null || !(Stats.GetStat<Stat_MovementSelector>(entity).Value is Movement_HomeToTarget))
        {
            spell.cleanup += Stats.GetStat<Stat_MovementSelector>(entity).AddModifier(new Movement_HomeToTarget(), 1);
        }
        (Stats.GetStat<Stat_MovementSelector>(entity).Value as Movement_HomeToTarget).homingRateDegreesPerSecond += 30f;
    }
}
*/