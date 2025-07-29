using System;

public enum EffectTargetSelector { None, Owner, Target }
[Flags]
public enum Alignment
{
    Neutral = 0,
    Buff = 1,
    Debuff = 2
}