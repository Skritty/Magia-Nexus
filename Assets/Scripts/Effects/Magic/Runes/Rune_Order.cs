using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune_Order : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public PersistentEffect debuff;

    [Header("Spell Shape")]
    [SerializeReference]
    public CreateEntity createSummons;
    public Dictionary<RuneElement, Action> summonRunes = new();
    public Action invoke;
    [SerializeReference]
    public PE_OverrideActions meleeOverride;

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        RemovePersistantEffect removeEffect = new RemovePersistantEffect();
        removeEffect.alignmentRemoved = damage.Owner.Stat<Stat_Team>().team == damage.Target.Stat<Stat_Team>().team ? PersistentEffect.Alignment.Debuff : PersistentEffect.Alignment.Buff;
        damage.onHitEffects.Add(removeEffect);
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Summon;
        spell.effect = createSummons.Clone();
        spell.cleanup += Trigger_SummonCreated.Subscribe(x => SetupSummon(spell, x.Entity), spell.effect);
    }

    private void SetupSummon(Spell spell, Entity entity)
    {
        for(int i = 1; i < spell.runes.Count; i++)
        {
            entity.Stat<Stat_Actions>().AddAction(summonRunes[spell.runes[i].element]);
        }
        switch (spell.runes[spell.runes.Count - 1].element)
        {
            case RuneElement.Fire:
            case RuneElement.Earth:
            case RuneElement.Order:
                {
                    meleeOverride.Create(meleeOverride);
                    break;
                }
        }
        Trigger_TurnComplete.Subscribe(x => invoke.OnStart(x.Entity));
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    //spell.proxyBlueprint.Stat<Stat_Magic>().maxStages += 2;
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
                    /*spell.lifetime += GameManager.Instance.ticksPerTurn * 2;
                    spell.castSpell.projectileFanAngle = 180f;
                    spell.castSpell.movementTarget = MovementTarget.Owner;
                    Movement_Orbit orbit = new Movement_Orbit();
                    orbit.orbitDistance = 1.75f;
                    spell.proxyBlueprint.Stat<Stat_Movement>().movementSelector = orbit;
                    spell.proxyBlueprint.Stat<Stat_Movement>().baseMovementSpeed = 4;*/
                    break;
                }
            case SpellShape.Summon:
                {
                    break;
                }
            case SpellShape.Curse:
                {
                    spell.additionalCastTargets++;
                    break;
                }
        }
    }
}
