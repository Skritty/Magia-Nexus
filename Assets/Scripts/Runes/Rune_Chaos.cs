using System.Collections;
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
    [FoldoutGroup("Projectile")]
    public Movement_HomeToTarget homing;
    [FoldoutGroup("Curse")]
    public Effect_DoHit curseHit;
    [FoldoutGroup("Curse")]
    public float baseChanneledCurseEffect;

    public override void MagicEffect(DamageInstance damage, int currentRuneIndex)
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
                pick.MagicEffect(damage, damage.runes.IndexOf(pick));
            }
        }
    }

    public override void Shape(Spell spell, int currentRuneIndex)
    {
        spell.shape = SpellShape.Curse;
        spell.effect = curseHit.Clone();
        spell.AddRunesToDamageInstance(spell.effect as DamageInstanceOLD);
        spell.proxies.Add(spell.Owner);
        spell.cleanup += Trigger_PersistentEffectLost.Subscribe(x => spell.StopSpell(), (spell.effect as DamageInstanceOLD).onHitEffects[0]);
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.cleanup += Trigger_SpellStageIncrement.Subscribe(SpellMultiPerStageFire, spell);
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

    private void SpellMultiPerStageFire(Spell spell)
    {
        spell.effect.effectMultiplier -= multiplierPerStage;
    }

    private void AddHomingToProjectile(Entity entity)
    {
        if (entity.Stat<Stat_MovementSelector>().Value == null || !(entity.Stat<Stat_MovementSelector>().Value is Movement_HomeToTarget))
        {
            entity.Stat<Stat_MovementSelector>().Value = new Movement_HomeToTarget();
        }
        (entity.Stat<Stat_MovementSelector>().Value as Movement_HomeToTarget).homingRateDegreesPerSecond += 30f;
    }
}
