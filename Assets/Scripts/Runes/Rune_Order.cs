using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rune/Order")]
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
    public float lifeMultiplier;
    public float damageMultiplier;

    public override void MagicEffect(DamageInstance damage, int currentRuneIndex)
    {
        damage.postOnHitEffects.Add(debuff);
    }

    public override void MagicEffectModifier(DamageInstance damage, int currentRuneIndex)
    {
        /*RemovePersistantEffect removeEffect = new RemovePersistantEffect();
        removeEffect.alignmentRemoved = damage.Owner.Stat<Stat_Team>().team == damage.Target.Stat<Stat_Team>().team ? PersistentEffect.Alignment.Debuff : PersistentEffect.Alignment.Buff;
        damage.onHitEffects.Add(removeEffect);*/
    }

    public override void Shape(Spell spell, int currentRuneIndex)
    {
        spell.shape = SpellShape.Summon;
        spell.effect = createSummons.Clone();
        Entity owner = spell.Owner;
        float lMult = lifeMultiplier;
        float dMult = damageMultiplier;
        if(owner.GetMechanic<Stat_PlayerOwner>().player != null)
            while (!owner.GetMechanic<Stat_PlayerOwner>().playerCharacter)
            {
                lMult *= lifeMultiplier;
                dMult *= damageMultiplier;
                owner = owner.GetMechanic<Stat_PlayerOwner>().proxyOwner;
            }
        (spell.effect as CreateEntity).lifeMultiplier = lMult;
        (spell.effect as CreateEntity).damageMultiplier = dMult;
        spell.cleanup += Trigger_SummonCreated.Subscribe(x => SetupSummon(spell, x.Entity), spell.effect);
        spell.proxies.Add(spell.Owner);
        spell.cleanup += Trigger_SpellMaxStage.Subscribe(x => x.Spell.StopSpell(), spell);
        spell.maxStages = 1;
    }

    private void SetupSummon(Spell spell, Entity entity)
    {
        entity.GetMechanic<Stat_Actions>().AddAction(move);
        for (int i = 1; i < spell.runes.Count; i++)
        {
            if (spell.runes[i].element == RuneElement.Null) continue;
            entity.GetMechanic<Stat_Actions>().AddAction(summonRunes[spell.runes[i].element]);
        }
        switch (spell.runes[spell.runes.Count - 1].element)
        {
            case RuneElement.Fire:
            case RuneElement.Earth:
            case RuneElement.Order:
                {
                    meleeOverride.Create(meleeOverride, spell.Owner, entity);
                    for (int i = 1; i < spell.runes.Count; i++)
                    {
                        entity.GetMechanic<Stat_Magic>().runes.Add(spell.runes[i]);
                    }
                    break;
                }
        }
        Trigger_TurnStart.Subscribe(Invoke, entity);
        spell.cleanup += Trigger_Die.Subscribe(y => SummonDeath(spell, entity), entity);
    }

    private void SummonDeath(Spell spell, Entity entity)
    {
        spell.Owner.GetMechanic<Stat_Team>().summons.Remove(entity);
        spell.Stage++;
    }

    private void Invoke(Trigger_TurnStart trigger)
    {
        invoke.DoEffects(trigger.Entity);
    }

    public override void ShapeModifier(Spell spell, int currentRuneIndex)
    {
        switch (spell.shape)
        {
            case SpellShape.Circle:
                {
                    spell.maxStages += 2;
                    break;
                }
            case SpellShape.Conjuration:
                {
                    break;
                }
            case SpellShape.Line:
                {
                    (spell.effect.targetSelector as Targeting_Line).numberOfTargets += 2;
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
