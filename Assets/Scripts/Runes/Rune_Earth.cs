using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Rune/Earth")]
public class Rune_Earth : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public Effect debuff;

    [Header("Spell Shape")]
    [SerializeReference]
    public CreateEntity createProjectiles;

    public override void MagicEffect(DamageInstance damage, int currentRuneIndex)
    {
        damage.postOnHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        PE_RuneCrystal crystal = new PE_RuneCrystal();
        crystal.rune = damage.runes[currentRuneIndex - 1];
        damage.postOnHitEffects.Add(crystal);
    }

    public override void Shape(Spell spell, int currentRuneIndex)
    {
        spell.shape = SpellShape.Projectile;
        spell.effect = createProjectiles.Clone();
        spell.SetToProjectile();
        (spell.effect as CreateEntity).projectileFanAngle = 30f;
        spell.proxies.Add(spell.Owner);
        spell.maxStages = 1;
        spell.cleanup += Trigger_ProjectileCreated.Subscribe(
            x => spell.cleanup += Trigger_Expire.Subscribe(y => ProjectileDeath(spell, y.Entity)));
        spell.cleanup += Trigger_SpellMaxStage.Subscribe(x => x.Spell.StopSpell(), spell);
    }

    private void ProjectileDeath(Spell spell, Entity entity)
    {
        spell.Stage++;
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    (spell.effect.targetSelector as Targeting_Radial).radius += 1;
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    break;
                }
            case SpellShape.Projectile:
                {
                    (spell.effect as CreateEntity).numberOfProjectiles += 2;
                    spell.maxStages += 2;
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