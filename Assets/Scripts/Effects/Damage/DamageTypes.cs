using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum DamageType
{
    TrueDamage = 1,
    Bludgeoning = 2,
    Slashing = 4,
    Piercing = 8,
    Magical = 16,
    Fire = 32,
    Lightning = 64,
    Cold = 128,
    Healing = 256,
}