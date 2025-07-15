using UnityEngine;

[CreateAssetMenu(menuName = "Rune/Water")]
public class Rune_Water : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public EffectTask buff;
    [SerializeReference]
    public EffectTask debuff;

    [Header("Spell Shape")]
    [SerializeReference]
    public PE_OverrideActions actionOverride;
    [SerializeReference]
    public Targeting multicastConeTargeting;

    public override void MagicEffect(DamageInstance damage, int currentRuneIndex)
    {
        damage.postOnHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        int index = (currentRuneIndex + 1) % damage.runes.Count;
        damage.runes[index].MagicEffect(damage, index);
        //damage.GenerateMagicEffect(damage.runes.Where(x => x.element != RuneElement.Water).ToList());
    }

    public override void Shape(Spell spell, int currentRuneIndex)
    {
        spell.shape = SpellShape.Conjuration;
        spell.effect = actionOverride.Clone();
        spell.cleanup += Trigger_PersistentEffectLost.Subscribe(x => spell.StopSpell(), spell.effect);
        spell.cleanup += Trigger_PreHit.Subscribe(x => AddMagicEffectRunesToAttackDamage(spell, x), spell.Owner, -5);
        spell.proxies.Add(spell.Owner);
    }

    private void AddMagicEffectRunesToAttackDamage(Spell spell, DamageInstanceOLD damage)
    {
        foreach(DamageSolver modifier in damage.damageModifiers)
        {
            if (modifier.method == NumericalModifierCalculationMethod.Flat && modifier.damageType.HasFlag(DamageType.Attack))
            {
                damage.runes.AddRange(spell.runes);
                return;
            }
        }
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    if((spell.effect.targetSelector as Targeting_Radial).angle == 180)
                    {
                        if(spell.proxies.Count != 0)
                        {
                            GameObject.Destroy(spell.proxies[0].gameObject);
                        }
                        spell.proxies.Add(spell.Owner);
                        (spell.effect.targetSelector as Targeting_Radial).angle = 30f;
                        (spell.effect.targetSelector as Targeting_Radial).radius 
                            = (spell.effect.targetSelector as Targeting_Radial).radius * 2;
                    }
                    (spell.effect.targetSelector as Targeting_Radial).angle += 15f;
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    //spell.castSpell.targetSelector = multicastConeTargeting;
                    //spell.castTargets += 2;
                    break;
                }
            case SpellShape.Projectile:
                {
                    //spell.castSpell.numberOfProjectiles += 4;
                    //spell.multiplier = 1f / spell.castSpell.numberOfProjectiles;
                    //spell.castSpell.projectileFanType = CreateEntity.ProjectileFanType.Shotgun;
                    //spell.castSpell.projectileFanAngle += 30f;
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