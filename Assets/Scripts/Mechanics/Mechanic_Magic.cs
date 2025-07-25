using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stat_SpellPhase : NumericalSolver, IStatTag<float> { }
public class Stat_Runes : ListStat<Rune>, IStatTag<Rune> { }
public class Stat_Enchantments : QueueStat<List<Rune>>, IStatTag<Queue<List<Rune>>> { }
public class Mechanic_Magic : Mechanic<Mechanic_Magic>
{
    public Spell originSpell;
    [SerializeReference, FoldoutGroup("Magic")]
    public List<Spell> ownedSpells = new List<Spell>();
    [FoldoutGroup("Magic")]
    public VFX vfx;
    private GraphicsBuffer runeInfo;

    protected override void Initialize()
    {
        if (vfx)
        {
            vfx = vfx.PlayVFX<VFX>(Owner.transform, Vector3.up * 1.5f, Vector3.zero, true);
            vfx.transform.parent = Owner.transform;
        }
    }

    public override void Tick()
    {
        if (Owner.Stat<Stat_Runes>().Count == 0)
        {
            vfx.visualEffect.enabled = false;
        }
    }

    public void AddRune(Rune rune)
    {
        if(vfx)
        {
            vfx.visualEffect.enabled = true;
        }
        Owner.Stat<Stat_Runes>().AddModifier(rune);
        Rune[] runes = Owner.Stat<Stat_Runes>().ToArray;
        Trigger_RuneGained.Invoke(rune, Owner, rune);
        vfx.visualEffect.SetInt("RuneCount", runes.Length);
        runeInfo?.Dispose();
        runeInfo = new GraphicsBuffer(GraphicsBuffer.Target.Structured, runes.Length, 4);
        float[] runeIndex = new float[runes.Length];
        for(int i = 0; i < runes.Length; i++)
        {
            runeIndex[i] = 1f * Array.IndexOf(Enum.GetValues(typeof(RuneElement)), runes[i].element);
        }
        runeInfo.SetData(runeIndex);
        vfx.visualEffect.SetGraphicsBuffer("Runes", runeInfo);
        vfx.visualEffect.SendEvent("SpawnRuneType");
    }
}

public enum SpellShape { Circle, Conjuration, Line, Projectile, Summon, Curse }

//--------------
//--------  ----
//-------    ---
//--------  ----
//--------------

// Rune                  Shape                      Effect/Effect Modifier     Spell Modifier
// Fire  (Damage):       Circle AoE (On Target),    Damage,                    Effect Multiplier
// Water (CC):           Cone AoE,                  CC,                        Mana Consumption
// Wind  (Utility):      Line AoE,                  Movement,                  Number of Projectiles/Number of Targets
// Earth (Combo):        Projectile,                Field Effects,             AoE Size
// Order (Defensive):    Circle AoE (On You),       ,                          Auto Target nearby ally (starting with you)
// Chaos (Persistent):   Auto Target,               Buff/Debuff,                    

// Metaspellcasting:
// Invoke: Start - mark AoEs/Targets, End - casts the spell
// Chant: End - Add your runes to world spell, then cast it each time the stack is filled
//                  On add rune to world spell: Cast spell, then remove last rune if over cap, then add your rune
// Invert: Inverts all Effect Modifiers or Spell Modifiers (depending on where in the spell it is placed)
// Extend: Activates another spell from the next rune using the first spell's effect trigger

// Damage Types:
// True: Order + Chaos
// Bludgeoning: Earth + Water
// Slashing: Earth + Wind
// Piercing: Water + Wind
// Magical: 3+ runes
// Fire: Fire
// Lightning: Wind
// Cold: Water
// Chaos: Chaos
// Healing: Order (does minor damage to enemies) (healing falls off the more an entity is healed)

// First rune: Effect
// Even rune (uses first if only 1 rune): Shape
// Odd rune (not first): Spell Modifier
// All runes (except for the first): Effect Modifier
// Effect, Modifier, Shape, Modifier, Shape, Modifer, Shape, Invoke
// Fire Fire Fire Fire Invoke

// Ignite Ground: fire, earth, fire, extend, chaos, invoke
// chaos, fire, earth, fire - Curse projectile of fire weakness
// Cleanse Ignite: order, order, order, fire
// Damage Buff: order, fire, order
// Chain Lightning: extend, fire, wind, wind, wind, chaos, invoke
// Thunderclap Totems: earth, earth, fire, wind, invoke

// Shapes
// Fire = Delayed Circle AoE (On Target)
// Water = Cone AoE (From Caster)
// Wind = Line AoE (From Caster)
// Earth = Projectile (From Caster)
// Order = Circle AoE Persistent Aura (On You)
// Chaos = Auto Hit Single Target (No AoE)

// Fire + Fire = Additional explosion after second delay
// Water + Water = Bigger Cone
// Wind + Wind = Longer Line
// Earth + Earth = More Projectiles
// Order + Order = Bigger circle
// Chaos + Chaos = More Targets

// Fire + Water = AoE circle that slowly moves forward while growing larger
// Fire + Wind = Bigger AoE circle that gradually moves forward
// Fire + Earth = Projectile that explodes on pierce (effect 1: projectile damage, effect 2: explosion)
// Fire + Order = Bigger AoE around you
// Fire + Chaos = AoE circle that tracks target

// Water + Wind = 
// Water + Earth = Projectile shotgun
// Water + Order = 
// Water + Chaos = Targets all entities that are targeting you

// Wind + Earth = Beam can now chain, +chains
// Wind + Order = AoE line linked between you and an ally
// Wind + Chaos = AoE line linked between you and an enemy

// Earth + Order = Projectiles orbit you
// Earth + Chaos = Perfect projectile target homing

// Order + Chaos = Global (effect scales on number of targets)

// ---------- EFFECT FORMULAS -----------
// (Effect Rune + Effect Type Rune [ignored]) + (Secondary Formula Runes [damage type determination])

// --Fire (damage type)--
// Enemy Targeted:
// (Damage Type Formula): Each damage type
// Ally Targeted:
// Healing

// --Water (CC type)--
// Enemy Targeted:
// Slow
// Knockback
// Charm
// Ally Targeted:

// --Wind (Utility)--
// Enemy Targeted:
// Reveal
// Inturrupt
// Ally Targeted:
// Invisibility
// Movement/blink

// --Earth (Field Effects)--
// Fire: DoT Ground
// Water: Ice Barrier (blocks movement and hits, Formed in AoE (or around the target))
// Wind: Wind Barrier (blocks or enhances projectiles passing through the barrier, Formed in AoE (or around the target))
// Earth: Totems (subtract the first two earth runes, totems have life and cast the remaining spell runes once a turn)
// Order: Minion (has life, chases after enemies, and does an attack based off of other runes in the formula)
// Chaos: 

// --Order ()--
// Enemy Targeted:
// Cleanse Buffs
// Ally Targeted:
// Cleanse Debuffs/DoT

// --Chaos (Buffs/Debuffs)--
// Fire/Wind/Chaos: Damage Buff/Debuff (Buff added damage types, in tiers based on the number of each rune)
// Earth/Water/Order: Defensive Buff/Debuff (Buff added defensive damage types, in tiers based on the number of each rune)

// NEW MAGIC FORMULA IDEA -----------------------------------------------------------------

// Fire - Damage | Effect: Damage Type | Creates Spell Shape
// Fire Fire Earth Earth
// 1. (Fire, Spell) -> Damage & Damage Type
// 2. (second rune) -> start shape formula
// 2. Fire -> Set shape to fire & do fire shape again (1st rune matches)
// 2. Earth -> Set shape projectile

// Earth - 
// Earth Invoke
// 1. (Earth) + None -> Dummy Totem

// Earth Earth Fire Fire
// 1. (Earth) + Earth -> Spell Totem
// 2. (second rune) -> New Spell Calc
// 2.1 (Fire, Spell) -> Damage & Damage Type
// 2.2 Fire -> Shape Calc

// Earth Order Fire Fire
// 1. (Earth) + Order -> Minion
// 2.

// Order Chaos

// Normal spell (Fire Fire Fire Fire Invoke): Delayed explosion happens twice at double damage
// Totem spell (Earth Earth Fire Fire Invoke): Totem that casts delayed explosion at 1x damage
// Minion spell (Earth Order Fire Fire Invoke): Minion that chases enemies and attacks for

// Normal spell: untargetable, less ways to scale
// Totem spell: has life, more ways to scale (ie. extra totems placed)

// Totems and minions override the shape formula
// Totems (Earth Earth) double next shape and effect rune
// Minions (Earth Order): First rune is AI type, second is attack type,
// third onward is effect (determined by the minion rune, ie. more minions summoned, faster attacks, more damage, more AoE, etc)