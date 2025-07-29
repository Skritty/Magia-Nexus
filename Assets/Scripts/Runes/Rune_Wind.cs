using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Rune/Wind")]
public class Rune_Wind : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public EffectTask buff;
    [SerializeReference]
    public EffectTask debuff;
    [SerializeReference]
    public EffectTask magicEffectModifier;

    [Header("Spell Shape")]
    public Effect_Damage lineEffect;

    public override void MagicEffect(DamageInstance damage, int currentRuneIndex)
    {
        //damage.postOnHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        //damage.postOnHitEffects.Add(magicEffectModifier);
    }

    public override void Shape(Spell spell, int currentRuneIndex)
    {
        spell.shape = SpellShape.Line;
        spell.effect = lineEffect.Clone();
        spell.proxies.Add(spell.Owner);
        spell.cleanup += Trigger_SpellCast.Subscribe(x => spell.StopSpell(), spell);
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.cleanup += Trigger_SpellCast.Subscribe(StageIncrease, spell);
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    spell.AddChaining(1);
                    spell.additionalCastTargets -= 1;
                    break;
                }
            case SpellShape.Projectile:
                {
                    //spell.proxyBlueprint.Stat<Stat_Projectile>().splitsRemaining += 1;
                    //spell.proxyBlueprint.Stat<Stat_Projectile>().piercesRemaining -= 1;
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

    private void StageIncrease(Spell spell)
    {
        //trigger.Spell.proxyBlueprint.Stat<Stat_Magic>().Stage++;
    }
}
