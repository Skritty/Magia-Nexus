using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Rune_Order : Rune
{
    [Header("Magic Effects")]
    [SerializeReference]
    public PersistentEffect buff;
    [SerializeReference]
    public PersistentEffect debuff;

    [Header("Spell Shape")]
    public CreateEntity createSummons;
    public SerializedDictionary<RuneElement, Action> summonRunes = new();
    public Action invoke;
    public Action move;
    public PE_OverrideActions meleeOverride;

    public override void MagicEffect(DamageInstance damage)
    {
        damage.onHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        /*RemovePersistantEffect removeEffect = new RemovePersistantEffect();
        removeEffect.alignmentRemoved = damage.Owner.Stat<Stat_Team>().team == damage.Target.Stat<Stat_Team>().team ? PersistentEffect.Alignment.Debuff : PersistentEffect.Alignment.Buff;
        damage.onHitEffects.Add(removeEffect);*/
    }

    public override void Shape(Spell spell)
    {
        spell.shape = SpellShape.Summon;
        spell.effect = createSummons.Clone();
        spell.cleanup += Trigger_SummonCreated.Subscribe(x => SetupSummon(spell, x.Entity), spell.effect);
        spell.proxies.Add(spell.Owner);
        spell.cleanup += Trigger_SpellMaxStage.Subscribe(x => x.Spell.StopSpell(), spell);
        spell.maxStages = 1;
    }

    private void SetupSummon(Spell spell, Entity entity)
    {
        entity.Stat<Stat_Actions>().AddAction(move);
        for (int i = 1; i < spell.runes.Count; i++)
        {
            entity.Stat<Stat_Actions>().AddAction(summonRunes[spell.runes[i].element]);
        }
        switch (spell.runes[spell.runes.Count - 1].element)
        {
            case RuneElement.Fire:
            case RuneElement.Earth:
            case RuneElement.Order:
                {
                    meleeOverride.Create(meleeOverride, Owner, entity);
                    for (int i = 1; i < spell.runes.Count; i++)
                    {
                        entity.Stat<Stat_Magic>().runes.Add(spell.runes[i]);
                    }
                    break;
                }
        }
        Trigger_TurnComplete.Subscribe(Invoke, entity);
        spell.cleanup += Trigger_Expire.Subscribe(y => SummonDeath(spell, y.Entity));
    }

    private void SummonDeath(Spell spell, Entity entity)
    {
        spell.Stage++;
    }

    private void Invoke(Trigger_TurnComplete trigger)
    {
        invoke.DoEffects(trigger.Entity);
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
