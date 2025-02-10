using System;

[Flags]
public enum DamageType
{
    // Parent Types
    None = 0, // 0000
    Damage = 1, // 0000
    Physical = 2, // 1000
    Elemental = 4, // 0100
    Divine = 8, // 0010

    // Physical
    Bludgeoning = (1 << 4) + 1 + 2, // Knocks back
    Slashing = (1 << 5) + 1 + 2, // Bleeds (scales off of base slashing damage)
    Piercing = (1 << 6) + 1 + 2, // +1 Piereces (Projectiles)/Hits Extra Targets (AoE, but reduced AoE Cone, longer AoE)
    // Elemental
    Fire = (1 << 7) + 1 + 4, // Ignites (scales off of base fire damage)
    Lightning = (1 << 8) + 1 + 4, // +1 Chains
    Cold = (1 << 9) + 1 + 4, // Slows
    // Divine
    Magical = (1 << 10) + 1 + 8, // Cannot be blocked
    Chaos = (1 << 11) + 1 + 8, // More damage per debuff on target
    Order = (1 << 12) + 1 + 8, // More healing per buff on target

    // Sources
    Attack = 1 << 13,
    Spell = 1 << 14,
    DoT = 1 << 15, 
    Projectile = 1 << 16, // Gains projectile behaviors (chain, multiple projectiles)
    AoE = 1 << 17,

    True = int.MaxValue
}

public enum EffectTag
{
    None,
    DamageTaken,
    DamageDealt,
    AoESize,
    Projectiles,
    Targets,
    Removeable,
    Knockback,
    MovementSpeed,
    Initiative,
    Enmity,
    SpellPhase,
}

// Calc (DamageDealt)
// Fire Whip: +100 (damage, elemental, fire, attack | hit)
// Triggers -> Torch: +10 (damage, elemental, fire, attack, DoT | DoT)
// Buff: +5 flat fire damage (applies to hit)
// Buff: Deal 10% more attack damage (applies to hit & DoT)

// Calc (Movement)
// Movement: +10 flat movement speed (none | Movement)
// Buff: 10% inc movement speed (none | Movement)