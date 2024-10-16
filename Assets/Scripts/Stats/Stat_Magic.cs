using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Magic : GenericStat<Stat_Magic>
{
    [SerializeReference, FoldoutGroup("Magic")]
    public List<Rune> runes = new List<Rune>(); // spellcasting queue
}

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
// Even rune: Spell Modifier
// Odd rune (except for the first if there are 3 or more runes): Shape
// All runes (except for the first): Effect Modifier
// Effect, Modifier, Shape, Modifier, Shape, Modifer, Shape, Invoke

// Ignite Ground: fire, earth, fire, extend, chaos, invoke
// chaos, fire, earth, fire - Curse projectile of fire weakness
// Cleanse Ignite: order, order, order, fire
// Damage Buff: order, fire, order
// Chain Lightning: extend, fire, wind, wind, wind, chaos, invoke
// Thunderclap Totems: earth, earth, fire, wind, invoke

// Shapes
// Fire + Fire = Bigger Circle
// Water + Water = Bigger Cone
// Wind + Wind = Longer Line
// Earth + Earth = More Projectiles
// Order + Order = Bigger circle
// Chaos + Chaos = More Targets

// Fire + Water = AoE circle that slowly moves forward while growing larger
// Fire + Wind = Bigger AoE circle that gradually moves forward
// Fire + Earth = Projectile AoE circle on target on hit
// Fire + Order = Bigger AoE around you
// Fire + Chaos = AoE circle that tracks target

// Water + Wind = 
// Water + Earth = Projectile shotgun
// Water + Order = 
// Water + Chaos = Targets all entities that are targeting you

// Wind + Earth = Projectiles have a trail that lingers
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